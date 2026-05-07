#!/usr/bin/env python3
"""Extract velocity topics to CSV and attach a Plotly visualization manifest."""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import json
import re
import sys
import traceback
import uuid
from pathlib import Path

import requests

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[4]
CLI_SRC_ROOT = REPO_ROOT / 'rdpms-cli'
DEV_TOOLS_ROOT = Path(__file__).resolve().parents[1]
for candidate in (CLI_SRC_ROOT, DEV_TOOLS_ROOT):
    if str(candidate) not in sys.path:
        sys.path.insert(0, str(candidate))

from common_rosbag_tooling import (
    append_tracker_row,
    build_client,
    build_tracker_path,
    create_target_dataset,
    download_dataset_files,
    ensure_tracker_header,
    find_schema_guid_by_urn,
    get_dataset_details,
    list_collection_datasets,
    load_failed_source_ids,
    load_successful_source_ids,
    metadata_id_to_uuid,
    resolve_download_uri,
    stamp_from_ns,
    upload_file_to_dataset,
)
from common_develop_tooling import add_develop_directory_options, add_retry_failed_option, temporary_directory
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.openapi_client.models.metadata_column_target_dto import MetadataColumnTargetDTO
from rdpms_cli.openapi_client.models.metadata_query_dto import MetadataQueryDTO
from rdpms_cli.openapi_client.models.metadata_query_part_dto import MetadataQueryPartDTO
from rdpms_cli.openapi_client.models.query_mode import QueryMode
from rdpms_cli.util.TypeStore import get_types

TSDATA_KEY = 'rdpms.tsdata'
VISUALIZATION_SCHEMA_URN = 'urn:rdpms:core:schema:visualization-manifest:v1'
TRACKER_FILENAME = 'cache/extracted_speed_csv_plotly.csv'
SPEED_TOPIC = '/speed'
FILTER_TOPIC = '/filter/twist'
SPEED_TYPE = 'std_msgs/msg/Float32'
TWIST_STAMPED_TYPE = 'geometry_msgs/msg/TwistStamped'
CSV_HEADERS = [
    'stamp',
    'speed',
    'filter_twist_linear_x',
    'filter_twist_linear_y',
    'filter_twist_linear_z',
]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description='Extract /speed Float32 and/or /filter/twist TwistStamped velocity topics to CSV and assign Plotly metadata.'
    )
    parser.add_argument('--source-collection', '-s', required=True, help='Source collection id containing bag datasets')
    parser.add_argument('--target-collection', '-t', required=True, help='Target collection id for generated CSV datasets')
    parser.add_argument(
        '--speed-topic',
        '--topic',
        dest='speed_topic',
        default=SPEED_TOPIC,
        help=f'Speed topic name (default: {SPEED_TOPIC})',
    )
    parser.add_argument(
        '--filter-topic',
        default=FILTER_TOPIC,
        help=f'Filter topic name (default: {FILTER_TOPIC})',
    )
    parser.add_argument(
        '--speed-topic-type',
        '--topic-type',
        dest='speed_topic_type',
        default=SPEED_TYPE,
        help=f'Speed topic ROS type (default: {SPEED_TYPE})',
    )
    parser.add_argument(
        '--filter-topic-type',
        default=TWIST_STAMPED_TYPE,
        help=f'Filter topic ROS type (default: {TWIST_STAMPED_TYPE})',
    )
    parser.add_argument(
        '--source-metadata-key',
        default=TSDATA_KEY,
        help=f'Metadata key used for topic/type query filter (default: {TSDATA_KEY})',
    )
    parser.add_argument(
        '--metadata-key',
        default='rdpms.viz',
        help='Visualization metadata key to assign on source dataset (default: rdpms.viz)',
    )
    parser.add_argument(
        '--schema-urn',
        default=VISUALIZATION_SCHEMA_URN,
        help=f'Schema URN used to validate assigned visualization metadata (default: {VISUALIZATION_SCHEMA_URN})',
    )
    parser.add_argument('--tracker-id', help='Optional tracker id to isolate processed-state for this workflow')
    parser.add_argument('--force', action='store_true', help='Process datasets even if tracker has status=success')
    add_retry_failed_option(parser)
    parser.add_argument('--limit', type=int, default=0, help='Optional max number of source datasets to process')
    parser.add_argument('--name-suffix', default='velocity-csv', help='Suffix for generated target dataset names')
    add_develop_directory_options(parser)
    return parser.parse_args()


def slugify(value: str) -> str:
    slug = re.sub(r'[^a-z0-9]+', '-', value.lower()).strip('-')
    return slug or 'velocity-csv'


def tracker_path(tracker_id: str | None, cache_dir: str | None) -> Path:
    return build_tracker_path(Path(__file__), TRACKER_FILENAME, tracker_id, cache_dir)


def rosbag_input_paths(downloaded_paths: list[Path]) -> list[Path]:
    metadata_files = [path for path in downloaded_paths if path.name == 'metadata.yaml']
    if metadata_files:
        return [metadata_files[0].parent]

    bag_files = [path for path in downloaded_paths if path.suffix.lower() in {'.mcap', '.db3', '.bag'}]
    if bag_files:
        return sorted(bag_files)
    if len(downloaded_paths) == 1:
        return downloaded_paths

    candidates = ', '.join(path.name for path in downloaded_paths)
    raise RuntimeError(f'could not determine rosbag input files from downloaded files: {candidates}')


def build_topic_query(topic_name: str, topic_type: str) -> dict[str, object]:
    return {
        'topics': {
            '$elemMatch': {
                'name': topic_name,
                'messageType.name': topic_type,
            }
        }
    }


def build_velocity_topic_query(topic_specs: list[tuple[str, str]]) -> dict[str, object]:
    return {
        '$or': [build_topic_query(topic_name, topic_type) for topic_name, topic_type in topic_specs]
    }


def query_eligible_dataset_ids(
    ds_api,
    *,
    source_collection_id: uuid.UUID,
    source_metadata_key: str,
    topic_specs: list[tuple[str, str]],
) -> set[str]:
    query_dto = MetadataQueryDTO(
        mode=QueryMode.AND,
        queries=[
            MetadataQueryPartDTO(
                metadata_key=source_metadata_key,
                target=MetadataColumnTargetDTO.DATASET,
                query=build_velocity_topic_query(topic_specs),
            )
        ],
    )
    candidates = ds_api.api_v1_data_datasets_post(collection_id=source_collection_id, metadata_query_dto=query_dto)
    return {str(ds.id) for ds in candidates if ds.id}


def extract_velocity_csv(
    *,
    bag_inputs: list[Path],
    output_path: Path,
    speed_topic: str,
    speed_topic_type: str,
    filter_topic: str,
    filter_topic_type: str,
) -> int:
    try:
        from rosbags.highlevel import AnyReader
    except ImportError as exc:
        raise RuntimeError('missing dependency: rosbags. Install it with `pip install rosbags`.') from exc

    row_count = 0
    topic_specs = [
        (speed_topic, speed_topic_type, 'speed'),
        (filter_topic, filter_topic_type, 'filter_twist'),
    ]
    topic_to_spec = {topic_name: (topic_type, prefix) for topic_name, topic_type, prefix in topic_specs}
    with AnyReader(bag_inputs) as reader:
        selected_connections = [
            conn
            for conn in reader.connections
            if conn.topic in topic_to_spec and conn.msgtype == topic_to_spec[conn.topic][0]
        ]
        if not selected_connections:
            raise RuntimeError(
                f'bag does not contain either {speed_topic}:{speed_topic_type} '
                f'or {filter_topic}:{filter_topic_type}'
            )

        missing_topics = [
            f'{topic_name}:{topic_type}'
            for topic_name, topic_type, _ in topic_specs
            if topic_name not in {conn.topic for conn in selected_connections}
        ]
        if missing_topics:
            missing = ', '.join(missing_topics)
            print(f'[warn] bag does not contain optional velocity topic(s), writing sparse CSV: {missing}')

        with output_path.open('w', newline='', encoding='utf-8') as f:
            writer = csv.writer(f)
            writer.writerow(CSV_HEADERS)
            for connection, timestamp, raw_data in reader.messages(connections=selected_connections):
                msg = reader.deserialize(raw_data, connection.msgtype)
                row = {header: '' for header in CSV_HEADERS}
                row['stamp'] = stamp_from_ns(timestamp)

                _, prefix = topic_to_spec[connection.topic]
                if prefix == 'speed':
                    row['speed'] = f'{float(msg.data):.9f}'
                else:
                    twist = getattr(msg, 'twist', None)
                    linear = getattr(twist, 'linear', None) if twist is not None else None
                    if linear is None:
                        raise RuntimeError(f'message on {connection.topic} does not contain twist.linear')
                    row[f'{prefix}_linear_x'] = f'{float(linear.x):.9f}'
                    row[f'{prefix}_linear_y'] = f'{float(linear.y):.9f}'
                    row[f'{prefix}_linear_z'] = f'{float(linear.z):.9f}'
                writer.writerow([row[header] for header in CSV_HEADERS])
                row_count += 1

    if row_count == 0:
        raise RuntimeError(f'topics {speed_topic} and {filter_topic} produced no velocity rows')
    return row_count


def build_velocity_visualization_item(
    *,
    source_dataset_name: str,
    csv_file_id: uuid.UUID,
) -> dict[str, object]:
    return {
        'title': 'Velocity CSV',
        'source': {'fileId': str(csv_file_id)},
        'renderer': {
            'kind': ['rdpms.timeseries-plotly', 'rdpms.table', 'rdpms.code'],
            'default': 'rdpms.timeseries-plotly',
            'options': {
                'timeField': 'stamp',
                'series': [
                    {'field': 'speed', 'label': '/speed'},
                    {'field': 'filter_twist_linear_x', 'label': '/filter/twist linear.x'},
                    {'field': 'filter_twist_linear_y', 'label': '/filter/twist linear.y'},
                    {'field': 'filter_twist_linear_z', 'label': '/filter/twist linear.z'},
                ],
                'mode': 'lines',
                'title': f'Velocity: {source_dataset_name}',
                'xAxisTitle': 'stamp',
                'yAxisTitle': 'linear velocity',
            },
        },
        'collapsible': False,
        'collapsedByDefault': False,
    }


def load_existing_visualization_manifest(
    meta_api,
    *,
    files_api,
    source_dataset_details,
    metadata_key: str,
) -> dict[str, object] | None:
    assigned = next(
        (
            item
            for item in (source_dataset_details.meta_dates or [])
            if (item.metadata_key or '').strip().lower() == metadata_key.strip().lower() and item.metadata_id
        ),
        None,
    )
    if not assigned:
        return None

    metadata = meta_api.api_v1_data_metadata_id_get(uuid.UUID(str(assigned.metadata_id)))
    if not metadata.file_id:
        return None

    file_id = uuid.UUID(str(metadata.file_id))
    file_summary = files_api.api_v1_data_files_id_get(file_id)
    download_uri = resolve_download_uri(files_api, file_id, file_summary.download_uri)
    response = requests.get(download_uri, allow_redirects=True, timeout=180)
    response.raise_for_status()

    manifest = response.json()
    if not isinstance(manifest, dict):
        return None
    return manifest


def build_or_extend_velocity_manifest(
    *,
    existing_manifest: dict[str, object] | None,
    source_dataset_name: str,
    csv_file_id: uuid.UUID,
) -> dict[str, object]:
    item = build_velocity_visualization_item(
        source_dataset_name=source_dataset_name,
        csv_file_id=csv_file_id,
    )

    if not existing_manifest:
        return {
            'id': str(uuid.uuid4()),
            'title': f'Velocity plot for {source_dataset_name}',
            'views': [
                {
                    'title': 'Velocity',
                    'items': [item],
                }
            ],
        }

    manifest = dict(existing_manifest)
    views = manifest.get('views')
    if not isinstance(views, list) or not views:
        manifest['views'] = [{'title': 'Velocity', 'items': [item]}]
        return manifest

    first_view = dict(views[0]) if isinstance(views[0], dict) else {'title': 'Velocity'}
    existing_items = first_view.get('items')
    items = list(existing_items) if isinstance(existing_items, list) else []

    # Replace an older generated velocity item from this workflow, but preserve other visualization items.
    items = [
        existing_item
        for existing_item in items
        if not (
            isinstance(existing_item, dict)
            and str(existing_item.get('title', '')).strip().lower()
            in {'/speed csv', 'velocity csv', 'twiststamped velocity csv'}
        )
    ]
    items.append(item)
    first_view['items'] = items
    views[0] = first_view
    manifest['views'] = views
    return manifest


def assign_velocity_visualization_manifest(
    ds_api,
    meta_api,
    *,
    files_api,
    source_dataset_id: uuid.UUID,
    source_dataset_details,
    source_dataset_name: str,
    csv_file_id: uuid.UUID,
    metadata_key: str,
    schema_guid: uuid.UUID | None,
) -> uuid.UUID:
    existing_manifest = load_existing_visualization_manifest(
        meta_api,
        files_api=files_api,
        source_dataset_details=source_dataset_details,
        metadata_key=metadata_key,
    )
    manifest = build_or_extend_velocity_manifest(
        existing_manifest=existing_manifest,
        source_dataset_name=source_dataset_name,
        csv_file_id=csv_file_id,
    )

    metadata_result = ds_api.api_v1_data_datasets_id_metadata_key_put(
        source_dataset_id,
        metadata_key,
        body=json.dumps(manifest).encode('utf-8'),
        _content_type='application/octet-stream',
    )
    metadata_id = metadata_id_to_uuid(metadata_result)

    if schema_guid:
        is_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(metadata_id, schema_guid)
        if not is_valid:
            print(f'[warn] metadata {metadata_id} did not validate against schema {schema_guid}')

    return metadata_id


def process_source_dataset(
    source_dataset,
    *,
    args: argparse.Namespace,
    ds_api,
    files_api,
    meta_api,
    types,
    target_collection_id: uuid.UUID,
    viz_schema_guid: uuid.UUID | None,
    dataset_index: int,
    dataset_total: int,
) -> tuple[str, str, str, str, int]:
    source_dataset_id = uuid.UUID(str(source_dataset.id))
    source_dataset_details = get_dataset_details(ds_api, source_dataset_id)
    source_dataset_name = str(source_dataset_details.name or source_dataset_id)

    with temporary_directory('rdpms-velocity-csv-', args.tmp_download_base_dir) as tmp:
        print(
            f'[info] downloading dataset ({dataset_index:02d}/{dataset_total:02d}): '
            f'{source_dataset_name} ({source_dataset_id})'
        )
        downloaded = download_dataset_files(source_dataset_details, files_api, tmp / 'input')
        bag_inputs = rosbag_input_paths(downloaded)
        csv_path = tmp / f'{slugify(source_dataset_name)}-velocity.csv'

        row_count = extract_velocity_csv(
            bag_inputs=bag_inputs,
            output_path=csv_path,
            speed_topic=args.speed_topic,
            speed_topic_type=args.speed_topic_type,
            filter_topic=args.filter_topic,
            filter_topic_type=args.filter_topic_type,
        )
        print(f'[info] extracted {row_count} velocity rows from {source_dataset_name}')

        target_dataset_id = create_target_dataset(
            ds_api,
            target_collection_id=target_collection_id,
            source_dataset_id=source_dataset_id,
            source_dataset_name=source_dataset_name,
            name_suffix=args.name_suffix,
        )
        csv_file_id = upload_file_to_dataset(ds_api, types, target_dataset_id, csv_path)
        ds_api.api_v1_data_datasets_id_seal_put(target_dataset_id)

        metadata_id = assign_velocity_visualization_manifest(
            ds_api,
            meta_api,
            files_api=files_api,
            source_dataset_id=source_dataset_id,
            source_dataset_details=source_dataset_details,
            source_dataset_name=source_dataset_name,
            csv_file_id=csv_file_id,
            metadata_key=args.metadata_key,
            schema_guid=viz_schema_guid,
        )
        print(f'[ok] assigned velocity Plotly manifest metadata={metadata_id} on source dataset={source_dataset_id}')

    return str(source_dataset_id), source_dataset_name, str(target_dataset_id), str(csv_file_id), row_count


def main() -> int:
    args = parse_args()
    source_collection_id = uuid.UUID(args.source_collection)
    target_collection_id = uuid.UUID(args.target_collection)

    tracker = tracker_path(args.tracker_id, args.cache_dir)
    ensure_tracker_header(
        tracker,
        [
            'processed_at_utc',
            'status',
            'source_dataset_id',
            'source_dataset_name',
            'target_dataset_id',
            'target_velocity_csv_file_id',
            'row_count',
            'message',
        ],
    )
    processed_success = load_successful_source_ids(tracker)
    processed_failed = load_failed_source_ids(tracker)

    client, ds_api, files_api, meta_api = build_client()
    types = get_types('', client)

    viz_schema_guid = find_schema_guid_by_urn(meta_api, args.schema_urn)
    if viz_schema_guid:
        print(f'[info] resolved schema GUID for {args.schema_urn}: {viz_schema_guid}')
    else:
        print(f'[warn] schema URN not found, visualization metadata will not be validated: {args.schema_urn}')

    source_datasets = list_collection_datasets(ds_api, source_collection_id)
    print(f'[info] found {len(source_datasets)} datasets in source collection {source_collection_id}')
    eligible_ids = query_eligible_dataset_ids(
        ds_api,
        source_collection_id=source_collection_id,
        source_metadata_key=args.source_metadata_key,
        topic_specs=[
            (args.speed_topic, args.speed_topic_type),
            (args.filter_topic, args.filter_topic_type),
        ],
    )
    source_datasets = [ds for ds in source_datasets if str(ds.id) in eligible_ids]
    print(
        f'[info] query filter matched {len(source_datasets)} dataset(s) for '
        f'{args.speed_topic}:{args.speed_topic_type} or {args.filter_topic}:{args.filter_topic_type}'
    )

    processed_count = 0
    success_count = 0
    skipped_count = 0
    skipped_failed_count = 0

    total_source_datasets = len(source_datasets)
    for dataset_index, source_dataset in enumerate(source_datasets, start=1):
        source_dataset_id = str(source_dataset.id)
        source_dataset_name = str(source_dataset.name or source_dataset_id)

        if not args.force and source_dataset_id in processed_success:
            skipped_count += 1
            print(f'[skip] already processed successfully: {source_dataset_name} ({source_dataset_id})')
            continue
        if not args.retry_failed and source_dataset_id in processed_failed:
            skipped_failed_count += 1
            print(
                f'[skip] previous processing failed, use --retry-failed to process again: '
                f'{source_dataset_name} ({source_dataset_id})'
            )
            continue
        if args.limit > 0 and processed_count >= args.limit:
            break

        processed_count += 1
        try:
            source_id, source_name, target_dataset_id, csv_file_id, row_count = process_source_dataset(
                source_dataset,
                args=args,
                ds_api=ds_api,
                files_api=files_api,
                meta_api=meta_api,
                types=types,
                target_collection_id=target_collection_id,
                viz_schema_guid=viz_schema_guid,
                dataset_index=dataset_index,
                dataset_total=total_source_datasets,
            )
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'success',
                    source_id,
                    source_name,
                    target_dataset_id,
                    csv_file_id,
                    str(row_count),
                    '',
                ],
            )
            success_count += 1
            print(f'[ok] source={source_id} -> velocity_csv_file={csv_file_id}')
        except Exception as exc:
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'failed',
                    source_dataset_id,
                    source_dataset_name,
                    '',
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
        f'skipped_failed={skipped_failed_count}, '
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
