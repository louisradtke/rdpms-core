#!/usr/bin/env python3
"""
This file adds or updates the schema index and assigns a new uuid for each new file.
Run it inside the schemas dir.
"""


import os
from pathlib import Path
from uuid import uuid4
import json

def main() -> None:
    schemas_path = Path('./schemas')
    json_dir = schemas_path / './json'
    uuid_list_json = schemas_path / 'schema_uuids.json'

    list_ = []
    if uuid_list_json.exists():
        list_ = json.loads(uuid_list_json.read_text())
        assert isinstance(list_, list)

    existing_parts = set(dct['schema'] for dct in list_)
    add_count = 0
    for part in json_dir.iterdir():
        part_str = str(part.relative_to(schemas_path).as_posix())
        if part_str in existing_parts:
            continue

        list_ += [{
            'schema': part_str,
            'id': str(uuid4())
        }]
        add_count += 1

    uuid_list_json.write_text(json.dumps(list_, indent=2) + '\n')
    print(f'Added {add_count} new schemas to {uuid_list_json}')

if __name__ == '__main__':
    main()
