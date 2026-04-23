#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd -- "$SCRIPT_DIR/../../.." && pwd)

# Map logical workflow stages to collection ids.
# The collections are the interface between independently running pipelines.
# Fill these in for your environment and keep the role names stable across workflows.
declare -A COLLECTIONS=(
  [raw]='00000000-0000-0000-0000-000000000000'
  [intermediate]='00000000-0000-0000-0000-000000000000'
  [derived]='00000000-0000-0000-0000-000000000000'
  [visualization]='00000000-0000-0000-0000-000000000000'
)

# Optional knobs shared across patterns.
PYTHON_BIN="${PYTHON_BIN:-python}"
FORCE_FLAG="${FORCE_FLAG:-0}"
LIMIT="${LIMIT:-0}"
SLEEP_SECONDS="${SLEEP_SECONDS:-30}"
RUN_FOREVER="${RUN_FOREVER:-0}"
MAX_CYCLES="${MAX_CYCLES:-1}"

PIPELINE_ORDER=()
declare -A PIPELINE_DESCRIPTIONS=()
declare -A PIPELINE_COMMANDS=()

log() {
  printf '[workflow] %s\n' "$*"
}

die() {
  printf '[workflow][fatal] %s\n' "$*" >&2
  exit 1
}

collection_id() {
  local role="$1"
  local value="${COLLECTIONS[$role]:-}"
  [[ -n "$value" ]] || die "unknown collection role: $role"
  [[ "$value" != "00000000-0000-0000-0000-000000000000" ]] || die "collection role '$role' is still unset"
  printf '%s\n' "$value"
}

run_tool() {
  local rel_script="$1"
  shift

  log "running $rel_script $*"
  "$PYTHON_BIN" "$REPO_ROOT/plugins/tools/develop/$rel_script" "$@"
}

maybe_force_args() {
  if [[ "$FORCE_FLAG" == "1" ]]; then
    printf '%s\n' '--force'
  fi
}

maybe_limit_args() {
  if [[ "$LIMIT" != "0" ]]; then
    printf '%s\n' '--limit' "$LIMIT"
  fi
}

register_pipeline() {
  local name="$1"
  local description="$2"
  local command="$3"

  PIPELINE_ORDER+=("$name")
  PIPELINE_DESCRIPTIONS["$name"]="$description"
  PIPELINE_COMMANDS["$name"]="$command"
}

run_pipeline() {
  local name="$1"
  local command="${PIPELINE_COMMANDS[$name]:-}"

  [[ -n "$command" ]] || die "pipeline '$name' is not registered"
  log "pipeline '$name': ${PIPELINE_DESCRIPTIONS[$name]}"
  eval "$command"
}

run_cycle() {
  local cycle_index="$1"
  local name

  log "starting cycle $cycle_index"
  for name in "${PIPELINE_ORDER[@]}"; do
    run_pipeline "$name"
  done
}

run_scheduler() {
  local cycle=1

  if [[ "${#PIPELINE_ORDER[@]}" -eq 0 ]]; then
    die 'no pipelines registered'
  fi

  if [[ "$RUN_FOREVER" == "1" ]]; then
    while true; do
      run_cycle "$cycle"
      cycle=$((cycle + 1))
      log "sleeping ${SLEEP_SECONDS}s before next cycle"
      sleep "$SLEEP_SECONDS"
    done
  fi

  while [[ "$cycle" -le "$MAX_CYCLES" ]]; do
    run_cycle "$cycle"
    cycle=$((cycle + 1))
    if [[ "$cycle" -le "$MAX_CYCLES" ]]; then
      log "sleeping ${SLEEP_SECONDS}s before next cycle"
      sleep "$SLEEP_SECONDS"
    fi
  done
}

tool_upload_debug_imu_csv() {
  local target_role="$1"
  shift || true

  run_tool \
    "upload_debug_imu_csv/upload_debug_imu_csv.py" \
    --collection "$(collection_id "$target_role")" \
    "$@"
}

tool_annotate_dataset_time_series_metadata() {
  local dataset_id="$1"
  shift || true

  run_tool \
    "annotate_dataset_time_series_metadata/annotate_dataset_time_series_metadata.py" \
    --dataset "$dataset_id" \
    "$@"
}

tool_process_collection_csv_to_viz() {
  local source_role="$1"
  local target_role="$2"
  shift 2 || true

  local args=(
    --source-collection "$(collection_id "$source_role")"
    --target-collection "$(collection_id "$target_role")"
  )

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_force_args)

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_limit_args)

  run_tool "process_collection_csv_to_viz/process_collection_csv_to_viz.py" "${args[@]}" "$@"
}

tool_extract_rosbag_gnss_imu_to_csv() {
  local source_role="$1"
  local target_role="$2"
  shift 2 || true

  local args=(
    --source-collection "$(collection_id "$source_role")"
    --target-collection "$(collection_id "$target_role")"
  )

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_force_args)

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_limit_args)

  run_tool "extract_rosbag_gnss_imu_to_csv/extract_rosbag_gnss_imu_to_csv.py" "${args[@]}" "$@"
}

declare_csv_demo_pipelines() {
  register_pipeline \
    'debug_csv_source' \
    'Create synthetic CSV source datasets in the raw collection' \
    "upload_debug_imu_csv raw"

  register_pipeline \
    'csv_to_visualization' \
    'Consume CSV datasets from raw and publish visualization datasets to visualization' \
    "process_collection_csv_to_viz raw visualization"
}

declare_rosbag_pipelines() {
  register_pipeline \
    'rosbag_to_intermediate_csv' \
    'Extract GNSS/IMU CSV artifacts from raw ROS bag datasets into intermediate' \
    "extract_rosbag_gnss_imu_to_csv raw intermediate"

  register_pipeline \
    'intermediate_csv_to_visualization' \
    'Consume intermediate CSV datasets and publish visualization datasets' \
    "process_collection_csv_to_viz intermediate visualization"
}

declare_join_demo_pipelines() {
  register_pipeline \
    'csv_quicklook' \
    'Produce quick-look visualizations directly from raw CSV datasets' \
    "process_collection_csv_to_viz raw visualization --limit 5"

  register_pipeline \
    'rosbag_branch' \
    'Produce intermediate CSV datasets from raw ROS bags' \
    "extract_rosbag_gnss_imu_to_csv raw intermediate"

  register_pipeline \
    'viz_from_intermediate' \
    'Produce visualizations from whatever intermediate CSV artifacts currently exist' \
    "process_collection_csv_to_viz intermediate visualization"
}

workflow_linear_csv_demo() {
  declare_csv_demo_pipelines
  run_scheduler
}

workflow_rosbag_csv_viz() {
  declare_rosbag_pipelines
  run_scheduler
}

workflow_collection_interface_demo() {
  declare_join_demo_pipelines
  run_scheduler
}

list_pipelines() {
  local name

  if [[ "${#PIPELINE_ORDER[@]}" -eq 0 ]]; then
    printf 'No pipelines declared yet.\n'
    return 0
  fi

  for name in "${PIPELINE_ORDER[@]}"; do
    printf '%s\t%s\n' "$name" "${PIPELINE_DESCRIPTIONS[$name]}"
  done
}

usage() {
  cat <<'EOF'
Usage:
  plugins/tools/develop/workflow_template.sh <command>

Commands:
  workflow_linear_csv_demo
  workflow_rosbag_csv_viz
  workflow_collection_interface_demo

Environment overrides:
  PYTHON_BIN=python3
  FORCE_FLAG=1
  LIMIT=10
  SLEEP_SECONDS=30
  MAX_CYCLES=5
  RUN_FOREVER=1

Core idea:
  Collections are the interface between pipelines. Each pipeline consumes from one
  collection and publishes into another. The scheduler just runs them cyclically.
  Correctness should come from idempotent tools plus stable collection wiring,
  not from a central orchestrator knowing the "right" execution order.

How to adapt:
  1. Replace the placeholder collection ids in the COLLECTIONS map.
  2. Add declare_*_pipelines functions that register independent pipelines.
  3. Use register_pipeline <name> <description> <command> for each pipeline.
  4. Compose a workflow_* function by declaring pipelines and calling run_scheduler.
  5. Use RUN_FOREVER=1 for a daemon-like cyclic execution model.
EOF
}

main() {
  local command="${1:-}"
  [[ -n "$command" ]] || {
    usage
    exit 1
  }

  shift || true

  case "$command" in
    workflow_linear_csv_demo|workflow_rosbag_csv_viz|workflow_collection_interface_demo)
      "$command" "$@"
      ;;
    help|-h|--help)
      usage
      ;;
    *)
      die "unknown command: $command"
      ;;
  esac
}

main "$@"
