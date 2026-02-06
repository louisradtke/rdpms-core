#!/bin/bash

# project-specific config
BASE_TYPES=(
  "visualization-manifest.v1.schema.json"
  "file-information.v1.schema.json"
)
OUTPUT_SUFFIX=".d.ts"
# copy-interface
SCHEMA_DIR="/work/json"
TARGET_DIR="/work/out"
  
set -ex

for schema in "${BASE_TYPES[@]}"; do
  base_name="${schema%.schema.json}"
  json2ts --cwd "$SCHEMA_DIR" -i "$SCHEMA_DIR/$schema" -o "$TARGET_DIR/${base_name}${OUTPUT_SUFFIX}"
done
