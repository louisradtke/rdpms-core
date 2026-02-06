#!/bin/bash

# pre-setup
export PATH="$PATH:/root/.dotnet/tools" >> ~/.bash_profile

# project-specific config
ROOT_NAMESPACE="RDPMS.Core.Contracts.Schemas"
BASE_TYPES=(
  "visualization-manifest.v1.schema.json"
  "file-information.v1.schema.json"
)
  
# generator-specific config, ensuring reproducibility in future versions
GEN_SCHEMA_VERSION="Draft202012"
GEN_OPTIONAL_AS_NULLABLE="NullOrUndefined"
GEN_ASSERT_FORMAT="True"

# copy-interface
SCHEMA_DIR="/work/json"
SCHEMA_TMP_DIR="/work/json_tmp"
TARGET_DIR="/work/out"

rm -rf "$SCHEMA_TMP_DIR"
mkdir -p "$SCHEMA_TMP_DIR"
cp "$SCHEMA_DIR"/*.schema.json "$SCHEMA_TMP_DIR"/

# rewrite $id to file URIs so relative refs resolve on disk
for schema in "$SCHEMA_TMP_DIR"/*.schema.json; do
  base="$(basename "$schema")"
  python3 - "$schema" "$SCHEMA_TMP_DIR/$base" <<'PY'
import json
import sys

path = sys.argv[1]
path_uri = sys.argv[2]

with open(path, "r", encoding="utf-8") as f:
    data = json.load(f)

data["$id"] = path_uri

def rewrite_refs(value, base_dir):
    if isinstance(value, dict):
        for k, v in list(value.items()):
            if k == "$ref" and isinstance(v, str):
                if v.endswith(".schema.json") and "://" not in v and not v.startswith("/"):
                    value[k] = f"{base_dir}/{v}"
            else:
                rewrite_refs(v, base_dir)
    elif isinstance(value, list):
        for item in value:
            rewrite_refs(item, base_dir)

rewrite_refs(data, sys.argv[2].rsplit("/", 1)[0])

with open(path, "w", encoding="utf-8") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
    f.write("\n")
PY
done

for schema in "${BASE_TYPES[@]}"; do
  generatejsonschematypes \
    "$SCHEMA_TMP_DIR/$schema" \
    --rootNamespace "$ROOT_NAMESPACE" \
    --outputPath "$TARGET_DIR" \
    --useSchema "$GEN_SCHEMA_VERSION" \
    --assertFormat "$GEN_ASSERT_FORMAT" \
    --optionalAsNullable "$GEN_OPTIONAL_AS_NULLABLE" \
    --useUnixLineEndings
done
