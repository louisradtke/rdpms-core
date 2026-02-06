#!/bin/bash

# pre-setup
export PATH="$PATH:/root/.dotnet/tools" >> ~/.bash_profile

# project-specific config
ROOT_NAMESPACE="RDPMS.Core.Contracts.Schemas"
BASE_TYPES=(
  "visualization-manifest.v1.schema.json"
  "file-information.v1.schema.json"
)
MAP_FILE_SUFFIX=".map.json"
  
# generator-specific config, ensuring reproducibility in future versions
GEN_SCHEMA_VERSION="Draft202012"
GEN_OPTIONAL_AS_NULLABLE="NullOrUndefined"
GEN_ASSERT_FORMAT="True"

# copy-interface
SCHEMA_DIR="/work/schemas"
TARGET_DIR="/work/out"

SCHEMA_TMP_DIR="/work/json_tmp"
UNIFIED_MAP_NAME="index.json"
SCHEMA_MAPS_DIR="/tmp/schema-maps"

rm -rf "$SCHEMA_TMP_DIR"
cp -r "$SCHEMA_DIR"/json/ "$SCHEMA_TMP_DIR"/

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

# copy source-of-truth schemas to output for embedding/packaging
cp -r "${SCHEMA_DIR}/json" "$TARGET_DIR"

mkdir "$SCHEMA_MAPS_DIR"
for schema in "${BASE_TYPES[@]}"; do
  generatejsonschematypes \
    "$SCHEMA_TMP_DIR/$schema" \
    --rootNamespace "$ROOT_NAMESPACE" \
    --outputPath "$TARGET_DIR" \
    --useSchema "$GEN_SCHEMA_VERSION" \
    --assertFormat "$GEN_ASSERT_FORMAT" \
    --optionalAsNullable "$GEN_OPTIONAL_AS_NULLABLE" \
    --outputMapFile "$SCHEMA_MAPS_DIR/${schema%.schema.json}${MAP_FILE_SUFFIX}" \
    --useUnixLineEndings
done

# generate one unified schema -> type map and enrich schema_uuids.json with typenames (if present)

UUID_FILE="$SCHEMA_DIR/schema_uuids.json"

python3 - "$TARGET_DIR" "$SCHEMA_MAPS_DIR" "$MAP_FILE_SUFFIX" "$SCHEMA_DIR/json" "$UNIFIED_MAP_NAME" "$UUID_FILE" <<'PY'
import glob
import json
import os
import re
import sys
from pathlib import Path

target_dir = Path(sys.argv[1])
schema_maps_dir = Path(sys.argv[2])
map_suffix = sys.argv[3]
schema_dir = Path(sys.argv[4])
unified_map_name = sys.argv[5]
uuid_file = Path(sys.argv[6]) if sys.argv[6] else None

map_files = sorted(schema_maps_dir.glob(f"*{map_suffix}"))
all_entries = []
for map_file in map_files:
    with map_file.open("r", encoding="utf-8") as f:
        all_entries.extend(json.load(f))

# key example: /work/json_tmp/visualization-manifest.v1.schema.json
root_key_pattern = re.compile(r"^.*/([^/]+\.schema\.json)$")
schema_to_type = {}
for entry in all_entries:
    key = entry.get("key", "")
    if "#" in key:
        continue
    m = root_key_pattern.match(key)
    if not m:
        continue
    schema_file = m.group(1)
    class_name = entry.get("class")
    if not class_name:
        continue
    schema_to_type[schema_file] = {
        "typeName": class_name.split(".")[-1],
        "className": class_name,
    }

if uuid_file and uuid_file.exists():
    with uuid_file.open("r", encoding="utf-8") as f:
        uuid_entries = json.load(f)

    enriched = []
    for item in uuid_entries:
        schema_ref = item.get("schema", "")
        schema_name = os.path.basename(schema_ref)
        type_info = schema_to_type.get(schema_name, {})
        enriched.append(
            {
                **item,
                "typeName": type_info.get("typeName"),
                "className": type_info.get("className"),
            }
        )

    with (target_dir / unified_map_name).open("w", encoding="utf-8") as f:
        json.dump(enriched, f, indent=2, ensure_ascii=False)
        f.write("\n")
PY
