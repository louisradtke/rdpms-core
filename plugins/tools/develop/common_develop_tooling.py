#!/usr/bin/env python3
"""Shared path helpers for prototype develop tools."""

from __future__ import annotations

import os
import tempfile
from contextlib import contextmanager
from pathlib import Path
from typing import Iterator

CACHE_DIR_ENV = 'RDPMS_TOOL_CACHE_DIR'
TMP_DOWNLOAD_BASE_DIR_ENV = 'RDPMS_TOOL_TMP_DOWNLOAD_BASE_DIR'


def add_develop_directory_options(parser) -> None:
    parser.add_argument(
        '--cache-dir',
        default=os.environ.get(CACHE_DIR_ENV),
        help=(
            'Directory for persistent tool cache/tracker files. '
            f'Defaults to each tool cache directory, or ${CACHE_DIR_ENV} when set.'
        ),
    )
    parser.add_argument(
        '--tmp-download-base-dir',
        '--temp-download-base-dir',
        dest='tmp_download_base_dir',
        default=os.environ.get(TMP_DOWNLOAD_BASE_DIR_ENV),
        help=(
            'Base directory for temporary downloaded/generated working files. '
            f'Defaults to the system temp directory, or ${TMP_DOWNLOAD_BASE_DIR_ENV} when set.'
        ),
    )


def build_cache_path(
    script_path: Path,
    default_filename: str,
    tracker_id: str | None = None,
    cache_dir: str | Path | None = None,
) -> Path:
    default_path = Path(default_filename)
    suffix = default_path.suffix or '.csv'
    filename = f'{default_path.stem}-{tracker_id}{suffix}' if tracker_id else default_path.name

    if cache_dir:
        base_dir = Path(cache_dir).expanduser()
    else:
        base_dir = script_path.resolve().parent / default_path.parent

    return base_dir / filename


def resolve_tmp_download_base_dir(base_dir: str | Path | None) -> Path | None:
    if not base_dir:
        return None

    path = Path(base_dir).expanduser()
    path.mkdir(parents=True, exist_ok=True)
    return path


@contextmanager
def temporary_directory(prefix: str, base_dir: str | Path | None = None) -> Iterator[Path]:
    resolved_base_dir = resolve_tmp_download_base_dir(base_dir)
    kwargs = {'dir': str(resolved_base_dir)} if resolved_base_dir else {}
    with tempfile.TemporaryDirectory(prefix=prefix, **kwargs) as tmp_dir:
        yield Path(tmp_dir)
