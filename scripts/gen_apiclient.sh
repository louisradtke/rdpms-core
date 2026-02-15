#!/bin/bash

########## settings ##########

# docker image
DOCKER_IMAGE="openapitools/openapi-generator-cli"

# paths where to copy the API clients
CLI_TARGET_DIR_RELATIVE_PATH="rdpms-cli/rdpms_cli/openapi_client"
WEB_TARGET_DIR_RELATIVE_PATH="rdpms-web-ui/src/lib/api_client"

# cli: client package name
CLI_PYTHON_PACKAGE_NAME="rdpms_cli.openapi_client"

########## setup ##########

# repository root
ROOT_DIR_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CLI_TARGET_DIR_PATH="$ROOT_DIR_PATH/$CLI_TARGET_DIR_RELATIVE_PATH"
WEB_TARGET_DIR_PATH="$ROOT_DIR_PATH/$WEB_TARGET_DIR_RELATIVE_PATH"

# get 8-char random string for tmp dir
TMP_DIR="/tmp/$(tr -dc A-Za-z0-9 </dev/urandom | head -c 8; echo)"

DOCKER_EXE="docker"
DOCKER_MOUNT_ARGS=""
DOCKER_RO_MOUNT_ARGS=":ro"

if command -v podman >/dev/null 2>&1; then
  DOCKER_EXE="podman"
  DOCKER_MOUNT_ARGS=":z"
  DOCKER_RO_MOUNT_ARGS=":ro,z"
fi

echo "using variables:"
echo "  DOCKER_IMAGE:            $DOCKER_IMAGE"
echo "  CLI_TARGET_DIR:          $CLI_TARGET_DIR_PATH"
echo "  WEB_TARGET_DIR:          $WEB_TARGET_DIR_PATH"
echo "  TMP_DIR:                 $TMP_DIR"
echo "  DOCKER_EXE:              $DOCKER_EXE"
echo "  DOCKER_MOUNT_ARGS:       $DOCKER_MOUNT_ARGS"
echo "  CLI_PYTHON_PACKAGE_NAME: $CLI_PYTHON_PACKAGE_NAME"
echo

########## generation ##########

TARGETS_RAW="${1:-cli}"
IFS=',' read -ra TARGETS <<< "$TARGETS_RAW"

generate_cli() {
  mkdir -p "$TMP_DIR/cli"

  "$DOCKER_EXE" run --rm \
    -v "$ROOT_DIR_PATH/swagger.yaml:/swagger.yaml$DOCKER_RO_MOUNT_ARGS" \
    -v "$TMP_DIR:/local$DOCKER_MOUNT_ARGS" \
    "$DOCKER_IMAGE" \
      generate \
      -g python \
        -i /swagger.yaml \
        -o /local/cli \
        "--additional-properties=packageName=$CLI_PYTHON_PACKAGE_NAME"

  python3 "$ROOT_DIR_PATH/scripts/fix_openapi_python_client.py" "$TMP_DIR/cli/rdpms_cli/openapi_client"

  rm -rf "$CLI_TARGET_DIR_PATH"
  cp -r "$TMP_DIR/cli/rdpms_cli/openapi_client" "$CLI_TARGET_DIR_PATH"
}

generate_web() {
  rm -rf "$WEB_TARGET_DIR_PATH"

  "$DOCKER_EXE" run --rm \
    -v "$ROOT_DIR_PATH:/repo$DOCKER_MOUNT_ARGS" \
    "$DOCKER_IMAGE" \
      generate \
      -g typescript-fetch \
        -i /repo/swagger.yaml \
        -o /repo/$WEB_TARGET_DIR_RELATIVE_PATH \
        --additional-properties=
}

for target in "${TARGETS[@]}"; do
  case "$target" in
    cli)
      set -ex  # set termination and verbosity
      generate_cli
      set +ex  # revert termination
      ;;
    web)
      set -ex  # set termination and verbosity
      generate_web
      set +ex  # revert termination
      ;;
    *)
      echo "unknown target: $target (supported: cli, web)" >&2
      exit 1
      ;;
  esac
done
