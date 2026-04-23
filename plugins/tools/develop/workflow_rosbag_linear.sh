#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd -- "$SCRIPT_DIR/../../.." && pwd)

# Map logical workflow stages to collection ids.
# The collections are the interface between independently running pipelines.
# Fill these in for your environment and keep the role names stable across workflows.
declare -A COLLECTIONS=(
  [raw]='c4269f73-0105-420c-a28d-5eb23bb69f9a'
  [truncated]='0c2577b9-e824-409d-8bdc-d4051612d4cc'
  [viz]='8291d4b8-d7db-4185-81bc-f8744474ef06'
)

# Optional knobs shared across patterns.
PYTHON_BIN="${PYTHON_BIN:-python}"
FORCE_FLAG="${FORCE_FLAG:-0}"
LIMIT="${LIMIT:-0}"
SLEEP_SECONDS="${SLEEP_SECONDS:-30}"
RUN_FOREVER="${RUN_FOREVER:-0}"
MAX_CYCLES="${MAX_CYCLES:-1}"
TRIM_CONFIG_PATH="${TRIM_CONFIG_PATH:-plugins/tools/develop/trim_motion_rosbag/config.yaml}"
declare -A TRACKER_IDS=(
  [annotate_raw]='rosbag-linear-annotate-raw-v1'
  [trim_motion]='rosbag-linear-trim-v1'
  [annotate_truncated]='rosbag-linear-annotate-truncated-v1'
  [extract_trimmed]='rosbag-linear-extract-trimmed-v1'
)

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

tracker_id() {
  local key="$1"
  local value="${TRACKER_IDS[$key]:-}"
  [[ -n "$value" ]] || die "unknown tracker id key: $key"
  printf '%s\n' "$value"
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

tool_annotate_rosbag_tsdata() {
  local source_role="$1"
  local tracker_key="$2"
  shift 2 || true

  local args=(
    --source-collection "$(collection_id "$source_role")"
    --tracker-id "$(tracker_id "$tracker_key")"
  )

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_force_args)

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_limit_args)

  run_tool "annotate_rosbag_tsdata/annotate_rosbag_tsdata.py" "${args[@]}" "$@"
}

tool_trim_motion_rosbag() {
  local source_role="$1"
  local target_role="$2"
  local tracker_key="$3"
  shift 3 || true

  local args=(
    --config "$TRIM_CONFIG_PATH"
    --source-collection "$(collection_id "$source_role")"
    --target-collection "$(collection_id "$target_role")"
    --tracker-id "$(tracker_id "$tracker_key")"
  )

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_force_args)

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_limit_args)

  run_tool "trim_motion_rosbag/trim_motion_rosbag.py" "${args[@]}" "$@"
}

tool_extract_rosbag_gnss_imu_to_csv() {
  local source_role="$1"
  local target_role="$2"
  local tracker_key="$3"
  shift 3 || true

  local args=(
    --source-collection "$(collection_id "$source_role")"
    --target-collection "$(collection_id "$target_role")"
    --tracker-id "$(tracker_id "$tracker_key")"
  )

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_force_args)

  while IFS= read -r arg; do
    args+=("$arg")
  done < <(maybe_limit_args)

  run_tool "extract_rosbag_gnss_imu_to_csv/extract_rosbag_gnss_imu_to_csv.py" "${args[@]}" "$@"
}

declare_rosbag_pipelines() {
  register_pipeline \
    'annotate_raw_rosbags' \
    'Annotate raw rosbag datasets with minimal rdpms.tsdata metadata' \
    "tool_annotate_rosbag_tsdata raw linear_annotate_raw"

  register_pipeline \
    'trim_motion_window' \
    'Trim rosbag datasets around detected movement and upload rewritten bags according to the trim config' \
    "tool_trim_motion_rosbag raw truncated trim_motion"

  register_pipeline \
    'annotate_trimmed_rosbags' \
    'Annotate trimmed rosbag datasets with minimal rdpms.tsdata metadata' \
    "tool_annotate_rosbag_tsdata truncated annotate_truncated"

  register_pipeline \
    'extract_viz' \
    'Annotate trimmed rosbag datasets with minimal rdpms.tsdata metadata' \
    "tool_extract_rosbag_gnss_imu_to_csv truncated viz tool_viz_truncated"
}

declare_join_demo_pipelines() {
  register_pipeline \
    'annotate_raw_rosbags' \
    'Annotate raw rosbag datasets with minimal rdpms.tsdata metadata' \
    "tool_annotate_rosbag_tsdata raw annotate_raw"

  register_pipeline \
    'trim_motion_window' \
    'Trim rosbag datasets around detected movement and upload rewritten bags according to the trim config' \
    "tool_trim_motion_rosbag raw truncated trim_motion"

  register_pipeline \
    'annotate_trimmed_rosbags' \
    'Annotate trimmed rosbag datasets with minimal rdpms.tsdata metadata' \
    "tool_annotate_rosbag_tsdata truncated annotate_truncated"

  register_pipeline \
    'extract_gnss_imu_from_trimmed' \
    'Extract GNSS/IMU CSV artifacts from the trimmed rosbag collection into the viz collection' \
    "tool_extract_rosbag_gnss_imu_to_csv truncated viz extract_trimmed"
}

workflow_rosbag_linear() {
  declare_rosbag_pipelines
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
  plugins/tools/develop/workflow_rosbag_linear.sh <command>

Commands:
  workflow_rosbag_linear
  workflow_rosbag_csv_viz
  workflow_collection_interface_demo

Environment overrides:
  PYTHON_BIN=python3
  FORCE_FLAG=1
  LIMIT=10
  SLEEP_SECONDS=30
  MAX_CYCLES=5
  RUN_FOREVER=1
  TRIM_CONFIG_PATH=plugins/tools/develop/trim_motion_rosbag/config.yaml

Core idea:
  Collections are the interface between pipelines. Each pipeline consumes from one
  collection and publishes into another. The scheduler just runs them cyclically.
  Correctness should come from idempotent tools plus stable collection wiring,
  not from a central orchestrator knowing the "right" execution order.

How to adapt:
  1. Keep the collection ids in the COLLECTIONS map aligned with the tool configs.
  2. Keep TRIM_CONFIG_PATH pointed at the concrete trim config you want to run.
  3. Register independent pipeline steps that consume and publish via collections.
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
    workflow_rosbag_linear|workflow_rosbag_csv_viz|workflow_collection_interface_demo)
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
