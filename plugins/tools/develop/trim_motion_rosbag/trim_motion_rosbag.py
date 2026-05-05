#!/usr/bin/env python3
"""Trim and rewrite rosbags around detected movement windows."""

from __future__ import annotations

import argparse
import datetime as dt
import json
import sys
import traceback
import uuid
from pathlib import Path

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[4]
CLI_SRC_ROOT = REPO_ROOT / 'rdpms-cli'
DEV_TOOLS_ROOT = Path(__file__).resolve().parents[1]
for candidate in (CLI_SRC_ROOT, DEV_TOOLS_ROOT):
    if str(candidate) not in sys.path:
        sys.path.insert(0, str(candidate))

from common_rosbag_tooling import (
    append_tracker_row,
    assign_time_series_metadata,
    build_client,
    build_tracker_path,
    build_minimal_time_series_metadata,
    create_target_dataset,
    download_dataset_files,
    ensure_tracker_header,
    find_schema_guid_by_urn,
    get_dataset_details,
    list_collection_datasets,
    load_successful_source_ids,
    summarize_time_series_topics,
    upload_file_to_dataset,
)
from common_develop_tooling import add_develop_directory_options, temporary_directory
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.openapi_client.models.metadata_column_target_dto import MetadataColumnTargetDTO
from rdpms_cli.openapi_client.models.metadata_query_dto import MetadataQueryDTO
from rdpms_cli.openapi_client.models.metadata_query_part_dto import MetadataQueryPartDTO
from rdpms_cli.openapi_client.models.query_mode import QueryMode
from rdpms_cli.util.TypeStore import get_types

TSDATA_KEY = 'rdpms.tsdata'
TSDATA_SCHEMA_URN = 'urn:rdpms:core:schema:time-series-container:v1'
TRACKER_FILENAME = 'cache/processed_trim_motion_rosbag.csv'


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description='Query rosbags by tsdata metadata, trim around movement, and upload rewritten bags.'
    )
    parser.add_argument('--config', '-c', required=True, help='Path to YAML config file')
    parser.add_argument('--source-collection', help='Override source collection id from the config file')
    parser.add_argument('--target-collection', help='Override target collection id from the config file')
    parser.add_argument('--tracker-id', help='Optional tracker id to isolate processed-state for this workflow')
    parser.add_argument('--force', action='store_true', help='Reprocess datasets even if tracker says success')
    parser.add_argument('--limit', type=int, default=0, help='Optional max number of datasets to process')
    add_develop_directory_options(parser)
    return parser.parse_args()


def load_yaml_config(path: Path) -> dict[str, object]:
    try:
        import yaml
    except ImportError as exc:
        raise RuntimeError('missing dependency: PyYAML. Install it with `pip install pyyaml`.') from exc

    with path.open('r', encoding='utf-8') as f:
        data = yaml.safe_load(f)
    if not isinstance(data, dict):
        raise RuntimeError(f'config {path} must be a YAML mapping')
    return data


def get_collection_id(config: dict[str, object], cli_value: str | None, key: str) -> uuid.UUID:
    if cli_value:
        return uuid.UUID(cli_value)
    return uuid.UUID(require_str(config, key))


def require_str(config: dict[str, object], key: str) -> str:
    value = config.get(key)
    if not isinstance(value, str) or not value.strip():
        raise RuntimeError(f'config key {key!r} must be a non-empty string')
    return value.strip()


def require_topics(config: dict[str, object], key: str) -> list[str]:
    value = config.get(key)
    if not isinstance(value, list) or not value:
        raise RuntimeError(f'config key {key!r} must be a non-empty list')
    topics = [str(item).strip() for item in value if str(item).strip()]
    if not topics:
        raise RuntimeError(f'config key {key!r} must contain at least one topic')
    return topics


def query_eligible_dataset_ids(ds_api, *, source_collection_id: uuid.UUID, source_metadata_key: str, topic_name: str, topic_type: str) -> set[str]:
    query_dto = MetadataQueryDTO(
        mode=QueryMode.AND,
        queries=[
            MetadataQueryPartDTO(
                metadata_key=source_metadata_key,
                target=MetadataColumnTargetDTO.DATASET,
                query={
                    'topics': {
                        '$elemMatch': {
                            'name': topic_name,
                            'messageType.name': topic_type,
                        }
                    }
                },
            )
        ],
    )
    candidates = ds_api.api_v1_data_datasets_post(collection_id=source_collection_id, metadata_query_dto=query_dto)
    return {str(ds.id) for ds in candidates if ds.id}


def find_motion_window_ns(
    *,
    bag_inputs: list[Path],
    topic_name: str,
    topic_type: str,
    movement_threshold: float,
    padding_seconds: float,
) -> tuple[int, int, int]:
    try:
        from rosbags.highlevel import AnyReader
    except ImportError as exc:
        raise RuntimeError('missing dependency: rosbags. Install it with `pip install rosbags`.') from exc

    first_bag_stamp_ns: int | None = None
    last_bag_stamp_ns: int | None = None
    first_motion_stamp_ns: int | None = None
    last_motion_stamp_ns: int | None = None

    with AnyReader(bag_inputs) as reader:
        matching_connections = [
            connection
            for connection in reader.connections
            if connection.topic == topic_name and connection.msgtype == topic_type
        ]
        if not matching_connections:
            raise RuntimeError(f'bag does not contain {topic_name} with type {topic_type}')

        for connection, raw_stamp_ns, raw_data in reader.messages():
            first_bag_stamp_ns = raw_stamp_ns if first_bag_stamp_ns is None else min(first_bag_stamp_ns, raw_stamp_ns)
            last_bag_stamp_ns = raw_stamp_ns if last_bag_stamp_ns is None else max(last_bag_stamp_ns, raw_stamp_ns)
            if connection.topic != topic_name or connection.msgtype != topic_type:
                continue

            msg = reader.deserialize(raw_data, connection.msgtype)
            value = float(getattr(msg, 'data'))
            if abs(value) > movement_threshold:
                first_motion_stamp_ns = raw_stamp_ns if first_motion_stamp_ns is None else min(first_motion_stamp_ns, raw_stamp_ns)
                last_motion_stamp_ns = raw_stamp_ns if last_motion_stamp_ns is None else max(last_motion_stamp_ns, raw_stamp_ns)

    if first_bag_stamp_ns is None or last_bag_stamp_ns is None:
        raise RuntimeError('bag does not contain any messages')
    if first_motion_stamp_ns is None or last_motion_stamp_ns is None:
        raise RuntimeError(
            f'no motion detected on {topic_name} using threshold {movement_threshold}'
        )

    padding_ns = int(padding_seconds * 1_000_000_000.0)
    start_ns = max(first_bag_stamp_ns, first_motion_stamp_ns - padding_ns)
    end_ns = min(last_bag_stamp_ns, last_motion_stamp_ns + padding_ns)
    return first_bag_stamp_ns, start_ns, end_ns


def collect_output_files(output_uri: Path) -> list[Path]:
    if output_uri.is_file():
        return [output_uri]
    if output_uri.is_dir():
        return sorted(path for path in output_uri.rglob('*') if path.is_file())
    raise RuntimeError(f'expected output bag at {output_uri}, but nothing was produced')


def rosbag_input_paths(downloaded_paths: list[Path]) -> list[Path]:
    metadata_files = [path for path in downloaded_paths if path.name == 'metadata.yaml']
    if metadata_files:
        return [metadata_files[0].parent]

    bag_files = [
        path for path in downloaded_paths if path.suffix.lower() in {'.mcap', '.db3', '.bag'}
    ]
    if bag_files:
        return sorted(bag_files)
    if len(downloaded_paths) == 1:
        return downloaded_paths

    candidates = ', '.join(path.name for path in downloaded_paths)
    raise RuntimeError(f'could not determine rosbag input files from downloaded files: {candidates}')


def storage_plugin_from_config(config: dict[str, object]):
    from rosbags.rosbag2 import StoragePlugin

    storage_id = str(config.get('output_storage_id', 'sqlite3')).strip().lower()
    if storage_id == 'mcap':
        return StoragePlugin.MCAP
    if storage_id == 'sqlite3':
        return StoragePlugin.SQLITE3
    raise RuntimeError(f'unsupported output_storage_id: {storage_id}')


def typestore_from_config(config: dict[str, object]):
    try:
        from rosbags.typesys import get_typestore
        from rosbags.typesys.stores import Stores
    except ImportError as exc:
        raise RuntimeError('missing dependency: rosbags. Install it with `pip install rosbags`.') from exc

    store_name = str(config.get('typestore', 'ROS2_JAZZY')).strip().upper()
    try:
        store = getattr(Stores, store_name)
    except AttributeError as exc:
        available = ', '.join(name for name in dir(Stores) if name.startswith('ROS2_'))
        raise RuntimeError(f'unknown typestore {store_name!r}; available: {available}') from exc

    return get_typestore(store)


def add_writer_connection(writer, conn, typestore):
    ext = conn.ext
    serialization_format = getattr(ext, 'serialization_format', 'cdr')
    offered_qos_profiles = getattr(ext, 'offered_qos_profiles', ())
    msgdef = getattr(conn.msgdef, 'data', None)
    rihs01 = conn.digest or None

    kwargs = {
        'serialization_format': serialization_format,
        'offered_qos_profiles': offered_qos_profiles,
    }
    if msgdef and rihs01:
        kwargs['msgdef'] = msgdef
        kwargs['rihs01'] = rihs01
    else:
        kwargs['typestore'] = typestore

    return writer.add_connection(conn.topic, conn.msgtype, **kwargs)


def connection_signature(conn) -> tuple[str, str, str, str]:
    ext = conn.ext
    serialization_format = getattr(ext, 'serialization_format', 'cdr')
    offered_qos_profiles = getattr(ext, 'offered_qos_profiles', ())
    return (
        conn.topic,
        conn.msgtype,
        serialization_format,
        repr(offered_qos_profiles),
    )


def write_filtered_rosbag(
    *,
    bag_inputs: list[Path],
    output_uri: Path,
    topics_to_keep: list[str],
    start_time_ns: int,
    end_time_ns: int,
    config: dict[str, object],
) -> int:
    try:
        from rosbags.highlevel import AnyReader
        from rosbags.rosbag2 import Writer
    except ImportError as exc:
        raise RuntimeError('missing dependency: rosbags. Install it with `pip install rosbags`.') from exc

    topic_filter = set(topics_to_keep)
    written_count = 0
    storage_plugin = storage_plugin_from_config(config)
    typestore = typestore_from_config(config)

    with AnyReader(bag_inputs) as reader, Writer(output_uri, version=9, storage_plugin=storage_plugin) as writer:
        conn_map = {}
        output_connections = {}
        selected_connections = []
        for conn in reader.connections:
            if conn.topic not in topic_filter:
                continue
            signature = connection_signature(conn)
            if signature not in output_connections:
                output_connections[signature] = add_writer_connection(writer, conn, typestore)
            conn_map[id(conn)] = output_connections[signature]
            selected_connections.append(conn)

        if not selected_connections:
            raise RuntimeError(f'none of the configured topics exist in the input bag: {sorted(topic_filter)}')

        for conn, timestamp, data in reader.messages(
            connections=selected_connections,
            start=start_time_ns,
            stop=end_time_ns + 1,
        ):
            writer.write(conn_map[id(conn)], timestamp, data)
            written_count += 1

    if written_count == 0:
        raise RuntimeError('filtered rosbag would contain no messages')

    return written_count


def tracker_path(tracker_id: str | None, cache_dir: str | None) -> Path:
    return build_tracker_path(Path(__file__), TRACKER_FILENAME, tracker_id, cache_dir)


def process_dataset(
    source_dataset,
    *,
    ds_api,
    files_api,
    meta_api,
    types,
    config: dict[str, object],
    ts_schema_guid,
    tmp_download_base_dir: str | None,
) -> tuple[str, str, str, str]:
    source_dataset_id = uuid.UUID(str(source_dataset.id))
    source_dataset_details = get_dataset_details(ds_api, source_dataset_id)
    source_dataset_name = str(source_dataset_details.name or source_dataset_id)

    with temporary_directory('rdpms-trim-motion-', tmp_download_base_dir) as tmp:
        input_dir = tmp / 'input'
        output_dir = tmp / 'output'
        output_dir.mkdir(parents=True, exist_ok=True)
        downloaded = download_dataset_files(source_dataset_details, files_api, input_dir)
        bag_inputs = rosbag_input_paths(downloaded)

        _bag_start_ns, start_time_ns, end_time_ns = find_motion_window_ns(
            bag_inputs=bag_inputs,
            topic_name=require_str(config, 'trigger_topic_name'),
            topic_type=require_str(config, 'trigger_topic_type'),
            movement_threshold=float(config.get('movement_threshold', 0.001)),
            padding_seconds=float(config.get('padding_seconds', 5.0)),
        )
        print(
            f'[info] movement window for {source_dataset_name}: '
            f'{start_time_ns} .. {end_time_ns}'
        )

        output_uri = output_dir / f'{source_dataset_id}-motion-window'
        written_count = write_filtered_rosbag(
            bag_inputs=bag_inputs,
            output_uri=output_uri,
            topics_to_keep=require_topics(config, 'topics_to_keep'),
            start_time_ns=start_time_ns,
            end_time_ns=end_time_ns,
            config=config,
        )
        print(f'[info] wrote filtered rosbag with {written_count} messages')

        output_files = collect_output_files(output_uri)
        topic_summary = summarize_time_series_topics(output_uri)
        metadata_doc = build_minimal_time_series_metadata(topic_summary)

        target_dataset_id = create_target_dataset(
            ds_api,
            target_collection_id=get_collection_id(config, None, 'target_collection'),
            source_dataset_id=source_dataset_id,
            source_dataset_name=source_dataset_name,
            name_suffix=str(config.get('name_suffix', 'trim')),
        )

        uploaded_file_ids: list[uuid.UUID] = []
        for output_file in output_files:
            uploaded_file_ids.append(upload_file_to_dataset(ds_api, types, target_dataset_id, output_file))

        assign_time_series_metadata(
            ds_api,
            meta_api,
            dataset_id=target_dataset_id,
            metadata_key=str(config.get('target_metadata_key', TSDATA_KEY)),
            metadata_doc=metadata_doc,
            schema_guid=ts_schema_guid,
        )
        ds_api.api_v1_data_datasets_id_seal_put(target_dataset_id)

    return (
        str(source_dataset_id),
        source_dataset_name,
        str(target_dataset_id),
        json.dumps([str(file_id) for file_id in uploaded_file_ids]),
    )


def main() -> int:
    args = parse_args()
    config_path = Path(args.config).resolve()
    config = load_yaml_config(config_path)

    source_collection_id = get_collection_id(config, args.source_collection, 'source_collection')
    target_collection_id = get_collection_id(config, args.target_collection, 'target_collection')
    source_metadata_key = str(config.get('source_metadata_key', TSDATA_KEY))

    config = dict(config)
    config['target_collection'] = str(target_collection_id)

    tracker = tracker_path(args.tracker_id, args.cache_dir)
    ensure_tracker_header(
        tracker,
        ['processed_at_utc', 'status', 'source_dataset_id', 'source_dataset_name', 'target_dataset_id', 'target_file_ids', 'message'],
    )
    processed_success = load_successful_source_ids(tracker)

    client, ds_api, files_api, meta_api = build_client()
    types = get_types('', client)
    ts_schema_guid = find_schema_guid_by_urn(meta_api, str(config.get('target_schema_urn', TSDATA_SCHEMA_URN)))
    if ts_schema_guid:
        print(f'[info] resolved schema GUID for {config.get("target_schema_urn", TSDATA_SCHEMA_URN)}: {ts_schema_guid}')
    else:
        print(f'[warn] schema URN not found, target metadata will not be validated')

    source_datasets = list_collection_datasets(ds_api, source_collection_id)
    print(f'[info] found {len(source_datasets)} datasets in source collection {source_collection_id}')
    eligible_ids = query_eligible_dataset_ids(
        ds_api,
        source_collection_id=source_collection_id,
        source_metadata_key=source_metadata_key,
        topic_name=require_str(config, 'trigger_topic_name'),
        topic_type=require_str(config, 'trigger_topic_type'),
    )
    source_datasets = [ds for ds in source_datasets if str(ds.id) in eligible_ids]
    print(
        f'[info] query filter matched {len(source_datasets)} dataset(s) for '
        f'{require_str(config, "trigger_topic_name")}:{require_str(config, "trigger_topic_type")}'
    )

    processed_count = 0
    success_count = 0
    skipped_count = 0

    for source_dataset in source_datasets:
        source_dataset_id = str(source_dataset.id)
        if not args.force and source_dataset_id in processed_success:
            skipped_count += 1
            print(f'[skip] already processed successfully: {source_dataset_id}')
            continue
        if args.limit > 0 and processed_count >= args.limit:
            break

        processed_count += 1
        try:
            source_id, source_name, target_dataset_id, target_file_ids = process_dataset(
                source_dataset,
                ds_api=ds_api,
                files_api=files_api,
                meta_api=meta_api,
                types=types,
                config=config,
                ts_schema_guid=ts_schema_guid,
                tmp_download_base_dir=args.tmp_download_base_dir,
            )
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'success',
                    source_id,
                    source_name,
                    target_dataset_id,
                    target_file_ids,
                    '',
                ],
            )
            success_count += 1
            print(f'[ok] source={source_id} -> target_dataset={target_dataset_id}')
        except Exception as exc:
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'failed',
                    source_dataset_id,
                    str(source_dataset.name or source_dataset_id),
                    '',
                    '',
                    str(exc).strip(),
                ],
            )
            print(f'[error] failed for dataset {source_dataset_id}: {exc}')
            print(traceback.format_exc())

    print(
        '[summary] '
        f'processed={processed_count}, success={success_count}, skipped_already_processed={skipped_count}, '
        f'tracker={tracker}'
    )
    return 0


if __name__ == '__main__':
    try:
        raise SystemExit(main())
    except ApiException as exc:
        print(f'[fatal] API error ({exc.status}): {exc.reason}')
        if exc.body:
            print(exc.body)
        raise SystemExit(1)
    except Exception as exc:
        print(f'[fatal] {exc}')
        print(traceback.format_exc())
        raise SystemExit(1)
