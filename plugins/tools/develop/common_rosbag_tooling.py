#!/usr/bin/env python3
"""Shared helpers for prototype rosbag-based develop tools."""

from __future__ import annotations

import csv
import datetime as dt
import hashlib
import json
import uuid
from pathlib import Path

import requests

from common_develop_tooling import build_cache_path, load_source_ids_by_status
from rdpms_cli.openapi_client.api_client import ApiClient
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.openapi_client.api.files_api import FilesApi
from rdpms_cli.openapi_client.api.meta_data_api import MetaDataApi
from rdpms_cli.openapi_client.configuration import Configuration
from rdpms_cli.openapi_client.models.data_set_create_request_dto import DataSetCreateRequestDTO
from rdpms_cli.openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
from rdpms_cli.util.config_store import load_file


def build_client() -> tuple[ApiClient, DataSetsApi, FilesApi, MetaDataApi]:
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        raise RuntimeError('unknown active instance key in rdpms_cli config')

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    Configuration.set_default(api_conf)
    ApiClient.set_default(ApiClient(api_conf))
    client = ApiClient.get_default()

    return client, DataSetsApi(client), FilesApi(client), MetaDataApi(client)


def find_schema_guid_by_urn(meta_api: MetaDataApi, schema_urn: str) -> uuid.UUID | None:
    schemas = meta_api.api_v1_data_schemas_get()
    for schema in schemas:
        if (schema.schema_id or '').strip().lower() == schema_urn.strip().lower() and schema.id:
            return uuid.UUID(str(schema.id))
    return None


def metadata_id_to_uuid(metadata_result: object) -> uuid.UUID:
    if hasattr(metadata_result, 'id'):
        metadata_value = getattr(metadata_result, 'id')
    else:
        metadata_value = metadata_result
    if metadata_value is None:
        raise RuntimeError('metadata assignment returned no metadata id')
    return uuid.UUID(str(metadata_value))


def create_dataset_id(ds_api: DataSetsApi, ds_req: DataSetCreateRequestDTO) -> uuid.UUID:
    created = ds_api.api_v1_data_datasets_new_post(ds_req)
    dataset_id = getattr(created, 'id', None)
    if not dataset_id:
        raise RuntimeError('dataset creation returned no id')
    return uuid.UUID(str(dataset_id))


def create_target_dataset(
    ds_api: DataSetsApi,
    *,
    target_collection_id: uuid.UUID,
    source_dataset_id: uuid.UUID,
    source_dataset_name: str,
    name_suffix: str,
) -> uuid.UUID:
    now_utc = dt.datetime.now(dt.timezone.utc)
    dataset_name = f'{source_dataset_name}-{name_suffix}-{str(source_dataset_id)[:8]}'
    request = DataSetCreateRequestDTO(
        name=dataset_name,
        slug=dataset_name,
        created_stamp_utc=now_utc,
        collection_id=target_collection_id,
    )
    return create_dataset_id(ds_api, request)


def upload_file_to_dataset(ds_api: DataSetsApi, types, dataset_id: uuid.UUID, file_path: Path) -> uuid.UUID:
    stats = file_path.stat()
    sha256 = hashlib.sha256()
    with file_path.open('rb') as f:
        for chunk in iter(lambda: f.read(65536), b''):
            sha256.update(chunk)

    content_type = types.resolve_by_ending(file_path.name)
    upload_req = S3FileCreateRequestDTO(
        name=file_path.name,
        size_bytes=stats.st_size,
        plain_sha256_hash=sha256.hexdigest(),
        created_stamp=dt.datetime.fromtimestamp(stats.st_ctime, dt.timezone.utc),
        content_type_id=content_type.id,
    )

    upload_resp = ds_api.api_v1_data_datasets_id_add_s3_post(dataset_id, upload_req)
    with file_path.open('rb') as f:
        response = requests.put(upload_resp.upload_uri, data=f, timeout=600)
    response.raise_for_status()

    if not upload_resp.file_id:
        raise RuntimeError('upload response did not include file id')
    return uuid.UUID(str(upload_resp.file_id))


def resolve_download_uri(files_api: FilesApi, file_id: uuid.UUID, inline_download_uri: str | None) -> str:
    if inline_download_uri:
        return inline_download_uri
    summary = files_api.api_v1_data_files_id_get(file_id)
    if summary.download_uri:
        return summary.download_uri
    raise RuntimeError(f'file {file_id} has no download URI')


def format_bytes(size_bytes: int | None) -> str:
    if size_bytes is None:
        return 'unknown size'

    units = ['B', 'KiB', 'MiB', 'GiB', 'TiB']
    size = float(size_bytes)
    unit = units[0]
    for unit in units:
        if abs(size) < 1024.0 or unit == units[-1]:
            break
        size /= 1024.0

    if unit == 'B':
        return f'{int(size)} {unit}'
    return f'{size:.1f} {unit}'


def download_dataset_files(dataset_detailed, files_api: FilesApi, target_dir: Path) -> list[Path]:
    target_dir.mkdir(parents=True, exist_ok=True)
    downloaded: list[Path] = []
    files = dataset_detailed.files or []
    if not files:
        raise RuntimeError('dataset has no files')

    total_files = len(files)
    for index, file in enumerate(files, start=1):
        if not file.id:
            raise RuntimeError('dataset file entry has no id')
        file_id = uuid.UUID(str(file.id))
        file_name = file.name or str(file_id)
        expected_size = getattr(file, 'size_bytes', None)
        print(
            f'[info] downloading file ({index:02d}/{total_files:02d}): '
            f'{file_name} ({format_bytes(expected_size)})'
        )
        download_uri = resolve_download_uri(files_api, file_id, file.download_uri)
        response = requests.get(download_uri, allow_redirects=True, timeout=600)
        response.raise_for_status()
        out_path = target_dir / file_name
        out_path.parent.mkdir(parents=True, exist_ok=True)
        out_path.write_bytes(response.content)
        print(
            f'[info] downloaded file ({index:02d}/{total_files:02d}): '
            f'{file_name} ({format_bytes(len(response.content))})'
        )
        downloaded.append(out_path)
    return downloaded


def detect_rosbag_input(downloaded_paths: list[Path]) -> Path:
    metadata_files = [path for path in downloaded_paths if path.name == 'metadata.yaml']
    if metadata_files:
        return metadata_files[0].parent

    preferred = [path for path in downloaded_paths if path.suffix.lower() in {'.mcap', '.db3', '.bag'}]
    if len(preferred) == 1:
        return preferred[0]
    if len(downloaded_paths) == 1:
        return downloaded_paths[0]

    candidates = ', '.join(path.name for path in downloaded_paths)
    raise RuntimeError(f'could not identify rosbag input from downloaded files: {candidates}')


def stamp_from_ns(ns_since_epoch: int) -> str:
    ts = ns_since_epoch / 1_000_000_000.0
    utc = dt.datetime.fromtimestamp(ts, tz=dt.timezone.utc)
    return utc.isoformat(timespec='milliseconds').replace('+00:00', 'Z')


def summarize_time_series_topics(bag_input: Path) -> dict[str, dict[str, object]]:
    try:
        from rosbags.highlevel import AnyReader
    except ImportError as exc:
        raise RuntimeError('missing dependency: rosbags. Install it with `pip install rosbags`.') from exc

    summary: dict[str, dict[str, object]] = {}
    with AnyReader([bag_input]) as reader:
        for connection, raw_stamp_ns, _raw_data in reader.messages():
            entry = summary.setdefault(
                connection.topic,
                {
                    'msgtype': connection.msgtype,
                    'count': 0,
                    'first_stamp_ns': raw_stamp_ns,
                    'last_stamp_ns': raw_stamp_ns,
                },
            )
            entry['count'] = int(entry['count']) + 1
            entry['first_stamp_ns'] = min(int(entry['first_stamp_ns']), raw_stamp_ns)
            entry['last_stamp_ns'] = max(int(entry['last_stamp_ns']), raw_stamp_ns)
    return summary


def build_minimal_time_series_metadata(topic_summary: dict[str, dict[str, object]]) -> dict[str, object]:
    topics: list[dict[str, object]] = []
    for topic_name in sorted(topic_summary.keys()):
        summary = topic_summary[topic_name]
        topics.append(
            {
                'name': topic_name,
                'metadata': {
                    'messageCount': int(summary['count']),
                    'firstMessageTimestamp': stamp_from_ns(int(summary['first_stamp_ns'])),
                    'lastMessageTimestamp': stamp_from_ns(int(summary['last_stamp_ns'])),
                },
                'messageType': {
                    'name': str(summary['msgtype']),
                    'reference': f'urn:ros2:msg:{summary["msgtype"]}',
                    'fields': []
                },
            }
        )
    return {'topics': topics}


def assign_time_series_metadata(
    ds_api: DataSetsApi,
    meta_api: MetaDataApi,
    *,
    dataset_id: uuid.UUID,
    metadata_key: str,
    metadata_doc: dict[str, object],
    schema_guid: uuid.UUID | None,
) -> uuid.UUID:
    metadata_result = ds_api.api_v1_data_datasets_id_metadata_key_put(
        dataset_id,
        metadata_key,
        body=json.dumps(metadata_doc).encode('utf-8'),
        _content_type='application/octet-stream',
    )
    metadata_id = metadata_id_to_uuid(metadata_result)

    if schema_guid:
        is_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(metadata_id, schema_guid)
        if not is_valid:
            raise RuntimeError(f'metadata {metadata_id} did not validate against schema {schema_guid}')

    return metadata_id


def list_collection_datasets(ds_api: DataSetsApi, collection_id: uuid.UUID) -> list[object]:
    return ds_api.api_v1_data_datasets_get(collection_id=collection_id)


def get_dataset_details(ds_api: DataSetsApi, dataset_id: uuid.UUID) -> object:
    return ds_api.api_v1_data_datasets_id_get(dataset_id)


def ensure_tracker_header(path: Path, header: list[str]) -> None:
    if path.exists():
        return
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(header)


def load_successful_source_ids(path: Path, source_field: str = 'source_dataset_id') -> set[str]:
    return load_source_ids_by_status(path, 'success', source_field)


def load_failed_source_ids(path: Path, source_field: str = 'source_dataset_id') -> set[str]:
    return load_source_ids_by_status(path, 'failed', source_field)


def append_tracker_row(path: Path, row: list[str]) -> None:
    with path.open('a', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(row)


def build_tracker_path(
    script_path: Path,
    default_filename: str,
    tracker_id: str | None,
    cache_dir: str | Path | None = None,
) -> Path:
    return build_cache_path(script_path, default_filename, tracker_id, cache_dir)
