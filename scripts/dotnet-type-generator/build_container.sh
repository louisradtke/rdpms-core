#!/bin/bash

# general settings

IMAGE_NAME="rdpms/dotnet-type-generator"

# gen variables

BUILD_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/" && pwd)"
GIT_SHORT_HASH="$(git rev-parse --short HEAD)"
IMAGE_NAME_LATEST="$IMAGE_NAME:latest"
IMAGE_NAME_GIT="$IMAGE_NAME:git-$GIT_SHORT_HASH"

# send it!

set -e

cd "$BUILD_PATH"

set -x

docker buildx build \
  -t "$IMAGE_NAME_LATEST" \
  -t "$IMAGE_NAME_GIT" \
   --load \
   .

set +x

echo "wrote container to: $IMAGE_NAME_GIT"
