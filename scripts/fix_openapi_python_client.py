#!/usr/bin/env python3
from __future__ import annotations

import argparse
import re
from pathlib import Path


CLASS_PATTERN = re.compile(r'^class\s+([A-Za-z0-9_]+)\(BaseModel\):', re.MULTILINE)
SELF_RECURSIVE_RETURN_PATTERN = re.compile(
    r'^(?P<indent>\s*)return import_module\("rdpms_cli\.openapi_client\.models\.(?P<module>[a-z0-9_]+)"\)\.(?P<class>[A-Za-z0-9_]+)\.from_dict\(obj\)\s*$'
)


def patch_model_file(path: Path) -> bool:
    source = path.read_text(encoding='utf-8')
    class_match = CLASS_PATTERN.search(source)
    if class_match is None:
        return False

    class_name = class_match.group(1)
    module_name = path.stem
    changed = False
    patched_lines = []

    for line in source.splitlines():
        match = SELF_RECURSIVE_RETURN_PATTERN.match(line)
        if match and match.group('module') == module_name and match.group('class') == class_name:
            patched_lines.append(f'{match.group("indent")}return cls.model_validate(obj)')
            changed = True
            continue
        patched_lines.append(line)

    if not changed:
        return False

    path.write_text('\n'.join(patched_lines) + '\n', encoding='utf-8')
    return True


def main() -> int:
    parser = argparse.ArgumentParser(description='Fix known self-recursive OpenAPI Python model deserialization bug.')
    parser.add_argument('client_root', type=Path, help='Path to generated openapi_client package root.')
    args = parser.parse_args()

    models_dir = args.client_root / 'models'
    if not models_dir.is_dir():
        raise SystemExit(f'Models directory not found: {models_dir}')

    patched_files = []
    for model_file in sorted(models_dir.glob('*.py')):
        if patch_model_file(model_file):
            patched_files.append(model_file)

    if patched_files:
        print('Patched self-recursive model from_dict calls:')
        for file in patched_files:
            print(f'  - {file}')
    else:
        print('No self-recursive model from_dict calls detected.')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
