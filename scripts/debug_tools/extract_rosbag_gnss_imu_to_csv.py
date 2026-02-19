#!/usr/bin/env python3
"""Debug helper: extract GNSS + IMU from ROS2 bag dataset, upload CSVs, annotate source dataset."""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import hashlib
import json
import re
import tempfile
import traceback
import uuid
from pathlib import Path
import sys

import requests

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[2]
CLI_SRC_ROOT = REPO_ROOT / 'rdpms-cli'
if str(CLI_SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(CLI_SRC_ROOT))

from rdpms_cli.openapi_client.api_client import ApiClient
from rdpms_cli.openapi_client.configuration import Configuration
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.openapi_client.api.files_api import FilesApi
from rdpms_cli.openapi_client.api.meta_data_api import MetaDataApi
from rdpms_cli.openapi_client.models.data_set_create_request_dto import DataSetCreateRequestDTO
from rdpms_cli.openapi_client.models.metadata_column_target_dto import MetadataColumnTargetDTO
from rdpms_cli.openapi_client.models.metadata_query_dto import MetadataQueryDTO
from rdpms_cli.openapi_client.models.metadata_query_part_dto import MetadataQueryPartDTO
from rdpms_cli.openapi_client.models.query_mode import QueryMode
from rdpms_cli.openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.util.TypeStore import get_types
from rdpms_cli.util.config_store import load_file

TSDATA_KEY = 'rdpms.tsdata'
TSDATA_SCHEMA_URN = 'urn:rdpms:core:schema:time-series-container:v1'
VISUALIZATION_SCHEMA_URN = 'urn:rdpms:core:schema:visualization-manifest:v1'
GNSS_TOPIC = '/gnss'
GNSS_TYPE = 'sensor_msgs/msg/NavSatFix'
IMU_TOPIC = '/imu/data'
IMU_TYPE = 'sensor_msgs/msg/Imu'
TRACKER_FILENAME = 'processed_rosbag_source_datasets.csv'


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            'Process ROS2 bag datasets from a source collection, extract /gnss and /imu/data to CSV, '
            'upload to a new dataset, and annotate source datasets with visualization metadata.'
        )
    )
    parser.add_argument('--source-collection', '-s', required=True, help='Source collection id containing ROS2 bag datasets')
    parser.add_argument('--target-collection', '-t', required=True, help='Target collection id for extracted CSV dataset')
    parser.add_argument(
        '--source-metadata-key',
        default=TSDATA_KEY,
        help=f'Metadata key used for topic/type query filter (default: {TSDATA_KEY})',
    )
    parser.add_argument(
        '--metadata-key',
        default='rdpms.viz',
        help='Metadata key for source visualization manifest assignment (default: rdpms.viz)',
    )
    parser.add_argument(
        '--schema-urn',
        default=VISUALIZATION_SCHEMA_URN,
        help=f'Schema URN used to validate assigned visualization metadata (default: {VISUALIZATION_SCHEMA_URN})',
    )
    parser.add_argument(
        '--name',
        help='Optional target dataset base name (default: generated from source dataset)',
    )
    parser.add_argument(
        '--force',
        action='store_true',
        help='Process datasets even if tracker has status=success',
    )
    parser.add_argument(
        '--limit',
        type=int,
        default=0,
        help='Optional max number of source datasets to process this run (0 = unlimited)',
    )
    return parser.parse_args()


def slugify(value: str) -> str:
    slug = re.sub(r'[^a-z0-9]+', '-', value.lower()).strip('-')
    return slug or 'ros2-csv-extract'


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
            return schema.id
    return None


def metadata_id_to_uuid(metadata_result: object) -> uuid.UUID:
    if hasattr(metadata_result, 'id'):
        metadata_value = getattr(metadata_result, 'id')
    else:
        metadata_value = metadata_result
    if metadata_value is None:
        raise RuntimeError('metadata assignment returned no metadata id')
    return uuid.UUID(str(metadata_value))


def build_topic_query() -> dict[str, object]:
    return {
        '$and': [
            {
                'topics': {
                    '$elemMatch': {
                        'name': GNSS_TOPIC,
                        'messageType.name': GNSS_TYPE,
                    }
                }
            },
            {
                'topics': {
                    '$elemMatch': {
                        'name': IMU_TOPIC,
                        'messageType.name': IMU_TYPE,
                    }
                }
            },
        ]
    }


def query_eligible_dataset_ids(
    ds_api: DataSetsApi,
    *,
    source_collection_id: uuid.UUID | None,
    source_metadata_key: str,
) -> set[str]:
    query_dto = MetadataQueryDTO(
        mode=QueryMode.AND,
        queries=[
            MetadataQueryPartDTO(
                metadata_key=source_metadata_key,
                target=MetadataColumnTargetDTO.DATASET,
                query=build_topic_query(),
            )
        ],
    )

    candidates = ds_api.api_v1_data_datasets_post(
        collection_id=source_collection_id,
        metadata_query_dto=query_dto,
    )
    return {str(ds.id) for ds in candidates if ds.id}


def get_dataset_details(ds_api: DataSetsApi, dataset_id: uuid.UUID):
    return ds_api.api_v1_data_datasets_id_get(dataset_id)


def list_collection_datasets(ds_api: DataSetsApi, collection_id: uuid.UUID) -> list[object]:
    return ds_api.api_v1_data_datasets_get(collection_id=collection_id)


def tracker_path() -> Path:
    return Path(__file__).resolve().parent / TRACKER_FILENAME


def ensure_tracker_header(path: Path) -> None:
    if path.exists():
        return

    with path.open('w', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(
            [
                'processed_at_utc',
                'status',
                'source_dataset_id',
                'source_dataset_name',
                'source_rosbag_file_id',
                'target_dataset_id',
                'target_gnss_file_id',
                'target_imu_file_id',
                'message',
            ]
        )


def load_successful_source_ids(path: Path) -> set[str]:
    if not path.exists():
        return set()

    success_ids: set[str] = set()
    with path.open('r', newline='', encoding='utf-8') as f:
        reader = csv.DictReader(f)
        for row in reader:
            if (row.get('status') or '').strip().lower() != 'success':
                continue
            source_id = (row.get('source_dataset_id') or '').strip()
            if source_id:
                success_ids.add(source_id)
    return success_ids


def append_tracker_row(
    path: Path,
    *,
    status: str,
    source_dataset_id: str,
    source_dataset_name: str,
    source_rosbag_file_id: str,
    target_dataset_id: str,
    target_gnss_file_id: str,
    target_imu_file_id: str,
    message: str,
) -> None:
    stamp = dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds')
    with path.open('a', newline='', encoding='utf-8') as f:
        writer = csv.writer(f)
        writer.writerow(
            [
                stamp,
                status,
                source_dataset_id,
                source_dataset_name,
                source_rosbag_file_id,
                target_dataset_id,
                target_gnss_file_id,
                target_imu_file_id,
                message,
            ]
        )


def find_rosbag_file(dataset_detailed) -> object:
    files = dataset_detailed.files or []
    if not files:
        raise RuntimeError('source dataset has no files')

    preferred = []
    fallback = []
    for file in files:
        name = (file.name or '').lower()
        mime = ((file.content_type and file.content_type.mime_type) or '').lower()
        abbr = ((file.content_type and file.content_type.abbreviation) or '').lower()

        if name.endswith(('.db3', '.mcap', '.bag')) or abbr == 'bag' or 'rosbag' in mime:
            preferred.append(file)
        else:
            fallback.append(file)

    if preferred:
        return preferred[0]
    if len(files) == 1:
        return files[0]

    candidate_names = ', '.join((f.name or '<unnamed>') for f in files)
    raise RuntimeError(
        'could not identify a rosbag file from dataset files; '
        f'available files: {candidate_names}'
    )


def resolve_download_uri(files_api: FilesApi, file_id: uuid.UUID, inline_download_uri: str | None) -> str:
    if inline_download_uri:
        return inline_download_uri
    summary = files_api.api_v1_data_files_id_get(file_id)
    if summary.download_uri:
        return summary.download_uri
    raise RuntimeError(f'file {file_id} has no download URI')


def stamp_from_ros_time(sec: int, nanosec: int) -> str:
    ts = sec + (nanosec / 1_000_000_000.0)
    utc = dt.datetime.fromtimestamp(ts, tz=dt.timezone.utc)
    return utc.isoformat(timespec='milliseconds').replace('+00:00', 'Z')


def stamp_from_ns(ns_since_epoch: int) -> str:
    ts = ns_since_epoch / 1_000_000_000.0
    utc = dt.datetime.fromtimestamp(ts, tz=dt.timezone.utc)
    return utc.isoformat(timespec='milliseconds').replace('+00:00', 'Z')


def scalar_type_name(value: object) -> str:
    if isinstance(value, bool):
        return 'bool'
    if isinstance(value, int):
        return 'int64'
    if isinstance(value, float):
        return 'float64'
    if isinstance(value, str):
        return 'string'
    return 'object'


def build_topic_field_definitions(msg: object) -> list[dict[str, object]]:
    fields: list[dict[str, object]] = []
    msg_values = vars(msg)
    for key, value in msg_values.items():
        field_type = scalar_type_name(value)
        if isinstance(value, list):
            if value:
                item_type = scalar_type_name(value[0])
                field_type = f'array<{item_type}>'
            else:
                field_type = 'array<object>'
        elif not isinstance(value, (bool, int, float, str)):
            field_type = 'object'

        fields.append(
            {
                'name': key,
                'type': {
                    'descriptor': f'ROS message field {key}',
                    'type': field_type,
                },
            }
        )
    return fields


def build_time_series_metadata_from_summary(topic_summary: dict[str, dict[str, object]]) -> dict[str, object]:
    topics: list[dict[str, object]] = []
    for topic_name in sorted(topic_summary.keys()):
        summary = topic_summary[topic_name]
        msgtype = str(summary['msgtype'])
        topics.append(
            {
                'name': topic_name,
                'metadata': {
                    'messageCount': int(summary['count']),
                    'firstMessageTimestamp': stamp_from_ns(int(summary['first_stamp_ns'])),
                    'lastMessageTimestamp': stamp_from_ns(int(summary['last_stamp_ns'])),
                },
                'messageType': {
                    'name': msgtype,
                    'description': f'ROS2 topic message type for {topic_name}',
                    'reference': f'urn:ros2:msg:{msgtype}',
                    'fields': list(summary['fields']),
                },
            }
        )
    return {'topics': topics}


def assign_time_series_metadata(
    ds_api: DataSetsApi,
    meta_api: MetaDataApi,
    *,
    source_dataset_id: uuid.UUID,
    metadata_key: str,
    metadata_doc: dict[str, object],
    schema_guid: uuid.UUID | None,
) -> uuid.UUID:
    metadata_result = ds_api.api_v1_data_datasets_id_metadata_key_put(
        source_dataset_id,
        metadata_key,
        body=json.dumps(metadata_doc).encode('utf-8'),
        _content_type='application/octet-stream',
    )
    metadata_id = metadata_id_to_uuid(metadata_result)

    if schema_guid:
        is_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(metadata_id, schema_guid)
        if not is_valid:
            print(f'[warn] metadata {metadata_id} did not validate against schema {schema_guid}')

    return metadata_id


def extract_csvs_from_rosbag(
    *,
    bag_path: Path,
    gnss_csv_path: Path,
    imu_csv_path: Path,
) -> tuple[int, int, dict[str, object]]:
    try:
        from rosbags.highlevel import AnyReader
    except ImportError as exc:
        raise RuntimeError(
            'missing dependency: rosbags. Install it with `pip install rosbags`.'
        ) from exc

    gnss_count = 0
    imu_count = 0
    topic_summary: dict[str, dict[str, object]] = {}

    with AnyReader([bag_path]) as reader:
        topic_types = {conn.topic: conn.msgtype for conn in reader.connections}

        if topic_types.get(GNSS_TOPIC) != GNSS_TYPE:
            raise RuntimeError(
                f'bag does not provide {GNSS_TOPIC} with expected type {GNSS_TYPE}; '
                f'got {topic_types.get(GNSS_TOPIC)!r}'
            )
        if topic_types.get(IMU_TOPIC) != IMU_TYPE:
            raise RuntimeError(
                f'bag does not provide {IMU_TOPIC} with expected type {IMU_TYPE}; '
                f'got {topic_types.get(IMU_TOPIC)!r}'
            )

        selected_connections = list(reader.connections)

        with gnss_csv_path.open('w', newline='', encoding='utf-8') as gnss_file, imu_csv_path.open(
            'w', newline='', encoding='utf-8'
        ) as imu_file:
            gnss_writer = csv.writer(gnss_file)
            imu_writer = csv.writer(imu_file)

            gnss_writer.writerow(
                [
                    'stamp',
                    'lat',
                    'lon',
                    'alt',
                    'status_service',
                    'status_status',
                    'position_covariance_type',
                ]
            )
            imu_writer.writerow(
                [
                    'stamp',
                    'orientation_x',
                    'orientation_y',
                    'orientation_z',
                    'orientation_w',
                    'angular_velocity_x',
                    'angular_velocity_y',
                    'angular_velocity_z',
                    'linear_acceleration_x',
                    'linear_acceleration_y',
                    'linear_acceleration_z',
                ]
            )

            for connection, raw_stamp_ns, raw_data in reader.messages(connections=selected_connections):
                msg = reader.deserialize(raw_data, connection.msgtype)
                if connection.topic not in topic_summary:
                    topic_summary[connection.topic] = {
                        'msgtype': connection.msgtype,
                        'count': 0,
                        'first_stamp_ns': raw_stamp_ns,
                        'last_stamp_ns': raw_stamp_ns,
                        'fields': build_topic_field_definitions(msg),
                    }
                entry = topic_summary[connection.topic]
                entry['count'] = int(entry['count']) + 1
                entry['first_stamp_ns'] = min(int(entry['first_stamp_ns']), raw_stamp_ns)
                entry['last_stamp_ns'] = max(int(entry['last_stamp_ns']), raw_stamp_ns)

                if connection.topic == GNSS_TOPIC:
                    if msg.header.stamp.sec == 0 and msg.header.stamp.nanosec == 0:
                        stamp = stamp_from_ns(raw_stamp_ns)
                    else:
                        stamp = stamp_from_ros_time(msg.header.stamp.sec, msg.header.stamp.nanosec)
                    gnss_writer.writerow(
                        [
                            stamp,
                            f'{msg.latitude:.9f}',
                            f'{msg.longitude:.9f}',
                            f'{msg.altitude:.4f}',
                            str(msg.status.service),
                            str(msg.status.status),
                            str(msg.position_covariance_type),
                        ]
                    )
                    gnss_count += 1
                elif connection.topic == IMU_TOPIC:
                    if msg.header.stamp.sec == 0 and msg.header.stamp.nanosec == 0:
                        stamp = stamp_from_ns(raw_stamp_ns)
                    else:
                        stamp = stamp_from_ros_time(msg.header.stamp.sec, msg.header.stamp.nanosec)
                    imu_writer.writerow(
                        [
                            stamp,
                            f'{msg.orientation.x:.9f}',
                            f'{msg.orientation.y:.9f}',
                            f'{msg.orientation.z:.9f}',
                            f'{msg.orientation.w:.9f}',
                            f'{msg.angular_velocity.x:.9f}',
                            f'{msg.angular_velocity.y:.9f}',
                            f'{msg.angular_velocity.z:.9f}',
                            f'{msg.linear_acceleration.x:.9f}',
                            f'{msg.linear_acceleration.y:.9f}',
                            f'{msg.linear_acceleration.z:.9f}',
                        ]
                    )
                    imu_count += 1

    return gnss_count, imu_count, build_time_series_metadata_from_summary(topic_summary)


def create_dataset_id(ds_api: DataSetsApi, ds_req: DataSetCreateRequestDTO) -> uuid.UUID:
    created = ds_api.api_v1_data_datasets_new_post(ds_req)
    dataset_id = getattr(created, 'id', None)
    if not dataset_id:
        raise RuntimeError('target dataset creation returned no id')
    return uuid.UUID(str(dataset_id))


def create_target_dataset(
    ds_api: DataSetsApi,
    target_collection_id: uuid.UUID,
    source_dataset_name: str,
    source_dataset_id: uuid.UUID,
    explicit_name: str | None,
) -> uuid.UUID:
    now_utc = dt.datetime.now(dt.timezone.utc)
    if explicit_name:
        dataset_name = f'{explicit_name}-{str(source_dataset_id)[:8]}'
    else:
        dataset_name = f'{str(source_dataset_id)}-{now_utc.strftime("%Y%m%d%H%M%S")}'
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
        response = requests.put(upload_resp.upload_uri, data=f)
    response.raise_for_status()

    if not upload_resp.file_id:
        raise RuntimeError('upload response did not include file id')
    return uuid.UUID(str(upload_resp.file_id))


def assign_visualization_manifest(
    ds_api: DataSetsApi,
    meta_api: MetaDataApi,
    *,
    source_dataset_id: uuid.UUID,
    metadata_key: str,
    source_dataset_name: str,
    gnss_file_id: uuid.UUID,
    imu_file_id: uuid.UUID,
    schema_guid: uuid.UUID | None,
) -> uuid.UUID:
    manifest = {
        'id': str(uuid.uuid4()),
        'title': f'ROS2 extracted views for {source_dataset_name}',
        'views': [
            {
                'title': 'Bag-derived artifacts',
                'items': [
                    {
                        'title': 'GNSS Track CSV',
                        'source': {'fileId': str(gnss_file_id)},
                        'renderer': {
                            'kind': ['rdpms.gps-track-svg', 'rdpms.table', 'rdpms.code'],
                            'default': 'rdpms.gps-track-svg',
                        },
                        'collapsible': False,
                        'collapsedByDefault': False,
                    },
                    {
                        'title': 'IMU CSV',
                        'source': {'fileId': str(imu_file_id)},
                        'renderer': {
                            'kind': ['rdpms.table', 'rdpms.code'],
                            'default': 'rdpms.table',
                        },
                        'collapsible': False,
                        'collapsedByDefault': False,
                    },
                ],
            }
        ],
    }

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
    ds_api: DataSetsApi,
    files_api: FilesApi,
    meta_api: MetaDataApi,
    types,
    target_collection_id: uuid.UUID,
    ts_schema_guid: uuid.UUID | None,
    viz_schema_guid: uuid.UUID | None,
) -> tuple[str, str, str, str, str, str]:
    source_dataset_id = uuid.UUID(str(source_dataset.id))
    source_dataset = get_dataset_details(ds_api, source_dataset_id)
    source_name = str(source_dataset.name or source_dataset_id)

    rosbag_file = find_rosbag_file(source_dataset)
    if not rosbag_file.id:
        raise RuntimeError('selected rosbag file has no id')

    rosbag_file_id = uuid.UUID(str(rosbag_file.id))
    download_uri = resolve_download_uri(files_api, rosbag_file_id, rosbag_file.download_uri)

    with tempfile.TemporaryDirectory(prefix='rdpms-ros2-csv-') as tmp_dir:
        tmp = Path(tmp_dir)

        rosbag_name = rosbag_file.name or f'{rosbag_file_id}.db3'
        bag_path = tmp / rosbag_name

        print(f'[info] downloading rosbag source file: {rosbag_name}')
        response = requests.get(download_uri, allow_redirects=True, timeout=180)
        response.raise_for_status()
        bag_path.write_bytes(response.content)

        gnss_csv = tmp / f'{slugify(source_name)}-gnss.csv'
        imu_csv = tmp / f'{slugify(source_name)}-imu.csv'

        print('[info] extracting /gnss and /imu/data to CSV files and summarizing all time-series topics')
        gnss_count, imu_count, ts_metadata = extract_csvs_from_rosbag(
            bag_path=bag_path,
            gnss_csv_path=gnss_csv,
            imu_csv_path=imu_csv,
        )
        print(f'[info] extracted messages: /gnss={gnss_count}, /imu/data={imu_count}')

        ts_metadata_id = assign_time_series_metadata(
            ds_api,
            meta_api,
            source_dataset_id=source_dataset_id,
            metadata_key=args.source_metadata_key,
            metadata_doc=ts_metadata,
            schema_guid=ts_schema_guid,
        )
        print(
            f'[info] assigned dynamic time-series metadata for source dataset: '
            f'metadata id={ts_metadata_id}, key={args.source_metadata_key}, topics={len(ts_metadata["topics"])}'
        )

        target_dataset_id = create_target_dataset(
            ds_api,
            target_collection_id,
            source_dataset_name=source_name,
            source_dataset_id=source_dataset_id,
            explicit_name=args.name,
        )
        print(f'[info] created target dataset: {target_dataset_id}')

        gnss_file_id = upload_file_to_dataset(ds_api, types, target_dataset_id, gnss_csv)
        imu_file_id = upload_file_to_dataset(ds_api, types, target_dataset_id, imu_csv)
        ds_api.api_v1_data_datasets_id_seal_put(target_dataset_id)
        print(f'[ok] uploaded CSV artifacts: gnss={gnss_file_id}, imu={imu_file_id}')

        metadata_id = assign_visualization_manifest(
            ds_api,
            meta_api,
            source_dataset_id=source_dataset_id,
            metadata_key=args.metadata_key,
            source_dataset_name=source_name,
            gnss_file_id=gnss_file_id,
            imu_file_id=imu_file_id,
            schema_guid=viz_schema_guid,
        )

    print(f'[ok] source dataset metadata assigned: metadata id={metadata_id}, key={args.metadata_key}')
    print(f'[ok] completed. source dataset={source_dataset_id}, target dataset={target_dataset_id}')
    return (
        str(source_dataset_id),
        source_name,
        str(rosbag_file_id),
        str(target_dataset_id),
        str(gnss_file_id),
        str(imu_file_id),
    )


def main() -> int:
    args = parse_args()

    source_collection_id = uuid.UUID(args.source_collection)
    target_collection_id = uuid.UUID(args.target_collection)

    tracker = tracker_path()
    ensure_tracker_header(tracker)
    processed_success = load_successful_source_ids(tracker)

    client, ds_api, files_api, meta_api = build_client()
    types = get_types('', client)

    ts_schema_guid = find_schema_guid_by_urn(meta_api, TSDATA_SCHEMA_URN)
    if ts_schema_guid:
        print(f'[info] resolved schema GUID for {TSDATA_SCHEMA_URN}: {ts_schema_guid}')
    else:
        print(f'[warn] schema URN not found, time-series metadata will not be validated: {TSDATA_SCHEMA_URN}')

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
    )
    source_datasets = [ds for ds in source_datasets if str(ds.id) in eligible_ids]
    print(
        '[info] query filter matched '
        f'{len(source_datasets)} dataset(s) for {GNSS_TOPIC}:{GNSS_TYPE} and {IMU_TOPIC}:{IMU_TYPE} '
        f'using metadata key {args.source_metadata_key}'
    )

    processed_count = 0
    success_count = 0
    skipped_count = 0

    for source_dataset in source_datasets:
        source_dataset_id = str(source_dataset.id)
        source_dataset_name = str(source_dataset.name or source_dataset_id)

        if not args.force and source_dataset_id in processed_success:
            skipped_count += 1
            print(f'[skip] already processed successfully: {source_dataset_name} ({source_dataset_id})')
            continue

        if args.limit > 0 and processed_count >= args.limit:
            break

        processed_count += 1
        print(f'[process] source dataset: {source_dataset_name} ({source_dataset_id})')

        try:
            source_id, source_name, source_rosbag_id, target_ds_id, target_gnss_id, target_imu_id = process_source_dataset(
                source_dataset,
                args=args,
                ds_api=ds_api,
                files_api=files_api,
                meta_api=meta_api,
                types=types,
                target_collection_id=target_collection_id,
                ts_schema_guid=ts_schema_guid,
                viz_schema_guid=viz_schema_guid,
            )
            append_tracker_row(
                tracker,
                status='success',
                source_dataset_id=source_id,
                source_dataset_name=source_name,
                source_rosbag_file_id=source_rosbag_id,
                target_dataset_id=target_ds_id,
                target_gnss_file_id=target_gnss_id,
                target_imu_file_id=target_imu_id,
                message='',
            )
            success_count += 1
            print(
                f'[ok] source={source_id} rosbag={source_rosbag_id} -> '
                f'target_dataset={target_ds_id}, gnss={target_gnss_id}, imu={target_imu_id}'
            )
        except Exception as exc:
            append_tracker_row(
                tracker,
                status='failed',
                source_dataset_id=source_dataset_id,
                source_dataset_name=source_dataset_name,
                source_rosbag_file_id='',
                target_dataset_id='',
                target_gnss_file_id='',
                target_imu_file_id='',
                message=str(exc).strip(),
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
