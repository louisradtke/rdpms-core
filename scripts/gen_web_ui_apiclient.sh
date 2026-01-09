#!/bin/bash

########## settings ##########

# docker image
DOCKER_IMAGE="openapitools/openapi-generator-cli"

# path where to copy the API client
TARGET_DIR_RELATIVE_PATH="rdpms-cli/rdpms_cli/openapi_client"

########## setup ##########

# repository root
ROOT_DIR_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TARGET_DIR_PATH="$ROOT_DIR_PATH/$TARGET_DIR_RELATIVE_PATH"

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
echo "  DOCKER_IMAGE:      $DOCKER_IMAGE"
echo "  TARGET_DIR_PATH:   $TARGET_DIR_PATH"
echo "  TMP_DIR:           $TMP_DIR"
echo "  DOCKER_EXE:        $DOCKER_EXE"
echo "  DOCKER_MOUNT_ARGS: $DOCKER_MOUNT_ARGS"
echo

# set termination and verbosity
set -ex

########## generation ##########

# generate TMP_DIR and containing api_client dir
mkdir -p "$TMP_DIR/api_client"

"$DOCKER_EXE" run --rm \
  -v "$ROOT_DIR_PATH/swagger.yaml:/swagger.yaml$DOCKER_RO_MOUNT_ARGS" \
  -v "$TMP_DIR:/local$DOCKER_MOUNT_ARGS" \
  "$DOCKER_IMAGE" \
    generate \
    -g python \
      -i /swagger.yaml \
      -o /local/api_client \
      --additional-properties=packageName=rdpms_cli.openapi_client

rm -r "$TARGET_DIR_PATH"
cp -r "$TMP_DIR/api_client/rdpms_cli/openapi_client" "$TARGET_DIR_PATH"
