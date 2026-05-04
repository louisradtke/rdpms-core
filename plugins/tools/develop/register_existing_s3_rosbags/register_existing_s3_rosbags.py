#!/usr/bin/env python3
"""Register existing S3 rosbag directories as sealed RDPMS datasets."""

from __future__ import annotations

import argparse
import datetime as dt
import re
import sys
import uuid
from dataclasses import dataclass
from pathlib import Path, PurePosixPath
from typing import Any

import requests

SLUG_RE = re.compile(r'^[A-Za-z0-9\.\+\-_]{1,128}$')

DEFAULT_EXTENSION_TYPES = {
    '.yaml': 'yaml',
    '.yml': 'yaml',
    '.mcap': 'mcap',
    '.bag': 'bag',
    '.db3': 'bag',
}


@dataclass(frozen=True)
class S3Object:
    key: str
    relative_key: str
    size: int
    last_modified: dt.datetime | None


@dataclass(frozen=True)
class RosbagCandidate:
    relative_dir: str
    dataset_name: str
    dataset_slug: str
    objects: tuple[S3Object, ...]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description='Register S3 rosbag directories, identified by metadata.yaml, as sealed RDPMS datasets.'
    )
    parser.add_argument('--config', '-c', required=True, help='Path to the main YAML config')
    parser.add_argument('--secrets', '-s', required=True, help='Path to the S3 store secrets YAML config')
    parser.add_argument('--dry-run', action='store_true', help='Only list intended registrations')
    parser.add_argument('--regex', help='Override discovery.directory_regex from the main config')
    parser.add_argument('--limit', type=int, default=0, help='Maximum number of candidates to register')
    return parser.parse_args()


def load_yaml(path: Path) -> dict[str, Any]:
    try:
        import yaml
    except ImportError as exc:
        raise RuntimeError('missing dependency: PyYAML. Install it with `pip install pyyaml`.') from exc

    with path.open('r', encoding='utf-8') as f:
        data = yaml.safe_load(f)
    if not isinstance(data, dict):
        raise RuntimeError(f'YAML file {path} must contain a mapping')
    return data


def require_mapping(config: dict[str, Any], key: str) -> dict[str, Any]:
    value = config.get(key)
    if not isinstance(value, dict):
        raise RuntimeError(f'config key {key!r} must be a mapping')
    return value


def require_str(config: dict[str, Any], key: str) -> str:
    value = config.get(key)
    if not isinstance(value, str) or not value.strip():
        raise RuntimeError(f'config key {key!r} must be a non-empty string')
    return value.strip()


def optional_str(config: dict[str, Any], key: str) -> str | None:
    value = config.get(key)
    if value is None:
        return None
    if not isinstance(value, str):
        raise RuntimeError(f'config key {key!r} must be a string when set')
    value = value.strip()
    return value or None


def normalize_base_url(value: str) -> str:
    return value.rstrip('/')


def normalize_prefix(value: str | None) -> str:
    if not value:
        return ''
    prefix = value.strip().lstrip('/')
    if prefix and not prefix.endswith('/'):
        prefix = f'{prefix}/'
    return prefix


def isoformat_utc(value: dt.datetime) -> str:
    if value.tzinfo is None:
        value = value.replace(tzinfo=dt.timezone.utc)
    return value.astimezone(dt.timezone.utc).isoformat(timespec='seconds').replace('+00:00', 'Z')


def get_store_config(secrets: dict[str, Any], store_id: str) -> dict[str, Any]:
    stores = require_mapping(secrets, 'stores')
    value = stores.get(store_id)
    if value is None:
        value = stores.get(str(uuid.UUID(store_id)))
    if not isinstance(value, dict):
        raise RuntimeError(f'secrets file has no store config for store {store_id}')
    return value


def build_s3_client(store_config: dict[str, Any]):
    try:
        import boto3
        from botocore.config import Config
    except ImportError as exc:
        raise RuntimeError('missing dependency: boto3. Install it with `pip install boto3`.') from exc

    endpoint_url = require_str(store_config, 'endpoint_url')
    region = optional_str(store_config, 'region')
    addressing_style = 'path' if store_config.get('use_path_style', True) else 'virtual'
    access_key = store_config.get('access_key') or store_config.get('access_key_id') or store_config.get('aws_access_key_id')
    secret_key = store_config.get('secret_key') or store_config.get('aws_secret_access_key')
    session_token = store_config.get('session_token') or store_config.get('aws_session_token')

    if not isinstance(access_key, str) or not access_key:
        raise RuntimeError('store credentials require access_key')
    if not isinstance(secret_key, str) or not secret_key:
        raise RuntimeError('store credentials require secret_key')

    return boto3.client(
        's3',
        endpoint_url=endpoint_url,
        region_name=region,
        aws_access_key_id=access_key,
        aws_secret_access_key=secret_key,
        aws_session_token=session_token,
        config=Config(signature_version='s3v4', s3={'addressing_style': addressing_style}),
    )


def list_s3_objects(client: object, bucket: str, prefix: str) -> list[S3Object]:
    paginator = client.get_paginator('list_objects_v2')
    objects: list[S3Object] = []
    for page in paginator.paginate(Bucket=bucket, Prefix=prefix):
        for item in page.get('Contents', []):
            key = str(item['Key'])
            if key == prefix or key.endswith('/'):
                continue
            if not key.startswith(prefix):
                continue
            objects.append(
                S3Object(
                    key=key,
                    relative_key=key[len(prefix):],
                    size=int(item['Size']),
                    last_modified=item.get('LastModified'),
                )
            )
    return objects


def discover_rosbag_candidates(
    objects: list[S3Object],
    prefix: str,
    directory_regex: str | None,
) -> list[RosbagCandidate]:
    pattern = re.compile(directory_regex) if directory_regex else None

    rosbag_dirs: list[str] = []
    for obj in objects:
        rel = obj.relative_key
        if rel == 'metadata.yaml':
            rosbag_dirs.append('')
        elif rel.endswith('/metadata.yaml'):
            rosbag_dirs.append(rel[:-len('/metadata.yaml')])

    candidates: list[RosbagCandidate] = []
    for relative_dir in sorted(set(rosbag_dirs)):
        if pattern and not pattern.search(relative_dir):
            continue

        if relative_dir:
            dir_prefix = f'{relative_dir}/'
            bag_objects = tuple(obj for obj in objects if obj.relative_key.startswith(dir_prefix))
            dataset_name = PurePosixPath(relative_dir).name
        else:
            bag_objects = tuple(objects)
            dataset_name = PurePosixPath(prefix.rstrip('/')).name or 'rosbag'

        candidates.append(
            RosbagCandidate(
                relative_dir=relative_dir,
                dataset_name=dataset_name,
                dataset_slug=dataset_name,
                objects=tuple(sorted(bag_objects, key=lambda obj: obj.relative_key)),
            )
        )
    return candidates


def get_existing_dataset_slugs(base_url: str, collection_id: str) -> set[str]:
    response = requests.get(
        f'{base_url}/api/v1/data/datasets',
        params={'collectionId': collection_id, 'deleted': 'Active,DeletionPending'},
        timeout=60,
    )
    response.raise_for_status()
    datasets = response.json()
    if not isinstance(datasets, list):
        raise RuntimeError('unexpected dataset list response from RDPMS')
    return {
        str(item.get('slug'))
        for item in datasets
        if isinstance(item, dict) and item.get('slug')
    }


def get_content_type_resolver(base_url: str, registration_config: dict[str, Any]):
    response = requests.get(f'{base_url}/api/v1/data/content-types', timeout=60)
    response.raise_for_status()
    content_types = response.json()
    if not isinstance(content_types, list):
        raise RuntimeError('unexpected content type list response from RDPMS')

    by_abbreviation: dict[str, str] = {}
    by_id: dict[str, str] = {}
    for item in content_types:
        if not isinstance(item, dict):
            continue
        type_id = item.get('id')
        abbreviation = item.get('abbreviation')
        if isinstance(type_id, str):
            by_id[str(uuid.UUID(type_id))] = type_id
        if isinstance(type_id, str) and isinstance(abbreviation, str):
            by_abbreviation[abbreviation.lower()] = type_id

    configured_extensions = registration_config.get('content_type_by_extension', {})
    if configured_extensions is None:
        configured_extensions = {}
    if not isinstance(configured_extensions, dict):
        raise RuntimeError('registration.content_type_by_extension must be a mapping when set')

    extension_types = DEFAULT_EXTENSION_TYPES.copy()
    for key, value in configured_extensions.items():
        ext = str(key).lower()
        if not ext.startswith('.'):
            ext = f'.{ext}'
        extension_types[ext] = str(value)

    fallback = str(registration_config.get('fallback_content_type', 'bag'))

    def resolve_value(value: str) -> str:
        try:
            normalized_id = str(uuid.UUID(value))
        except ValueError:
            normalized_id = ''

        if normalized_id:
            if normalized_id not in by_id:
                raise RuntimeError(f'content type id {value} does not exist in RDPMS')
            return by_id[normalized_id]

        content_type_id = by_abbreviation.get(value.lower())
        if not content_type_id:
            raise RuntimeError(f'content type abbreviation {value!r} does not exist in RDPMS')
        return content_type_id

    fallback_id = resolve_value(fallback)

    def resolve(file_name: str) -> str:
        suffix = PurePosixPath(file_name).suffix.lower()
        configured = extension_types.get(suffix)
        if configured:
            return resolve_value(configured)
        return fallback_id

    return resolve


def relative_name_inside_bag(candidate: RosbagCandidate, obj: S3Object) -> str:
    if not candidate.relative_dir:
        return obj.relative_key
    prefix = f'{candidate.relative_dir}/'
    return obj.relative_key[len(prefix):]


def build_registration_payload(
    candidate: RosbagCandidate,
    *,
    collection_id: str,
    store_id: str,
    resolve_content_type,
) -> dict[str, Any]:
    created_values = [obj.last_modified for obj in candidate.objects if obj.last_modified is not None]
    created_stamp = min(created_values) if created_values else dt.datetime.now(dt.timezone.utc)

    return {
        'name': candidate.dataset_name,
        'slug': candidate.dataset_slug,
        'createdStampUTC': isoformat_utc(created_stamp),
        'collectionId': collection_id,
        'storeId': store_id,
        'files': [
            {
                'name': relative_name_inside_bag(candidate, obj),
                'objectKey': obj.relative_key,
                'contentTypeId': resolve_content_type(obj.relative_key),
                'sizeBytes': obj.size,
                'createdStampUTC': isoformat_utc(obj.last_modified or created_stamp),
            }
            for obj in candidate.objects
        ],
    }


def register_candidate(base_url: str, payload: dict[str, Any]) -> str:
    response = requests.post(
        f'{base_url}/api/v1/data/datasets/new/sealed/s3',
        json=payload,
        timeout=600,
    )
    if response.status_code >= 400:
        try:
            message = response.json().get('message')
        except Exception:
            message = response.text
        raise RuntimeError(f'RDPMS rejected dataset {payload["slug"]!r}: {message}')
    body = response.json()
    dataset_id = body.get('id') if isinstance(body, dict) else None
    if not dataset_id:
        raise RuntimeError(f'RDPMS response for dataset {payload["slug"]!r} did not contain an id')
    return str(dataset_id)


def main() -> int:
    args = parse_args()
    config = load_yaml(Path(args.config))
    secrets = load_yaml(Path(args.secrets))

    rdpms_config = require_mapping(config, 'rdpms')
    discovery_config = config.get('discovery') or {}
    registration_config = config.get('registration') or {}
    if not isinstance(discovery_config, dict):
        raise RuntimeError('config key discovery must be a mapping when set')
    if not isinstance(registration_config, dict):
        raise RuntimeError('config key registration must be a mapping when set')

    base_url = normalize_base_url(require_str(rdpms_config, 'base_url'))
    collection_id = str(uuid.UUID(require_str(rdpms_config, 'collection_id')))
    store_id = str(uuid.UUID(require_str(config, 's3_store_id')))
    directory_regex = args.regex if args.regex is not None else optional_str(discovery_config, 'directory_regex')

    store_config = get_store_config(secrets, store_id)
    bucket = require_str(store_config, 'bucket')
    prefix = normalize_prefix(optional_str(store_config, 'prefix'))
    s3_client = build_s3_client(store_config)

    print(f'[scan] bucket={bucket} prefix={prefix!r}')
    objects = list_s3_objects(s3_client, bucket, prefix)
    print(f'[scan] found {len(objects)} objects below prefix')

    candidates = discover_rosbag_candidates(objects, prefix, directory_regex)
    print(f'[scan] found {len(candidates)} rosbag candidates')

    if args.limit > 0:
        candidates = candidates[:args.limit]

    existing_slugs = get_existing_dataset_slugs(base_url, collection_id)
    resolve_content_type = get_content_type_resolver(base_url, registration_config)
    seen_slugs: set[str] = set()

    registered = 0
    skipped = 0
    failed = 0

    for candidate in candidates:
        label = candidate.relative_dir or '.'
        if not SLUG_RE.match(candidate.dataset_slug):
            print(f'[skip] {label}: dataset slug {candidate.dataset_slug!r} is invalid for RDPMS')
            skipped += 1
            continue

        if candidate.dataset_slug in existing_slugs:
            print(f'[skip] {label}: dataset slug {candidate.dataset_slug!r} already exists')
            skipped += 1
            continue

        if candidate.dataset_slug in seen_slugs:
            print(f'[skip] {label}: dataset slug {candidate.dataset_slug!r} was already selected in this run')
            skipped += 1
            continue

        seen_slugs.add(candidate.dataset_slug)
        try:
            payload = build_registration_payload(
                candidate,
                collection_id=collection_id,
                store_id=store_id,
                resolve_content_type=resolve_content_type,
            )
        except Exception as exc:
            print(f'[fail] {label}: {exc}')
            failed += 1
            continue

        if args.dry_run:
            print(
                f'[dry-run] {label}: would register slug={candidate.dataset_slug!r} '
                f'files={len(candidate.objects)} size={sum(obj.size for obj in candidate.objects)}'
            )
            registered += 1
            continue

        try:
            dataset_id = register_candidate(base_url, payload)
        except Exception as exc:
            print(f'[fail] {label}: {exc}')
            failed += 1
            continue

        existing_slugs.add(candidate.dataset_slug)
        registered += 1
        print(f'[ok] {label}: registered dataset {dataset_id}')

    action = 'would-register' if args.dry_run else 'registered'
    print(f'[summary] {action}={registered} skipped={skipped} failed={failed}')
    return 1 if failed else 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except KeyboardInterrupt:
        print('interrupted', file=sys.stderr)
        raise SystemExit(130)
    except Exception as exc:
        print(f'error: {exc}', file=sys.stderr)
        raise SystemExit(1)
