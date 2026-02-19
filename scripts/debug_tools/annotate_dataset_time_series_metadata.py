#!/usr/bin/env python3
"""Debug helper: assign ROS2 time-series metadata to a dataset."""

from __future__ import annotations

import argparse
import datetime as dt
import json
import traceback
import uuid
from pathlib import Path
import sys

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[2]
CLI_SRC_ROOT = REPO_ROOT / 'rdpms-cli'
if str(CLI_SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(CLI_SRC_ROOT))

from rdpms_cli.openapi_client.api_client import ApiClient
from rdpms_cli.openapi_client.configuration import Configuration
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.openapi_client.api.meta_data_api import MetaDataApi
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.util.config_store import load_file

TSDATA_SCHEMA_URN = 'urn:rdpms:core:schema:time-series-container:v1'
TSDATA_KEY = 'rdpms.tsdata'
GNSS_TOPIC = '/gnss'
GNSS_TYPE = 'sensor_msgs/msg/NavSatFix'
IMU_TOPIC = '/imu/data'
IMU_TYPE = 'sensor_msgs/msg/Imu'


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description='Create and assign time-series metadata to a dataset.'
    )
    parser.add_argument('--dataset', '-d', required=True, help='Target dataset id')
    parser.add_argument(
        '--metadata-key',
        default=TSDATA_KEY,
        help=f'Metadata key to assign (default: {TSDATA_KEY})',
    )
    parser.add_argument(
        '--schema-urn',
        default=TSDATA_SCHEMA_URN,
        help=f'Schema URN to validate against (default: {TSDATA_SCHEMA_URN})',
    )
    parser.add_argument(
        '--gnss-count',
        type=int,
        default=0,
        help='Message count hint for /gnss topic metadata (default: 0)',
    )
    parser.add_argument(
        '--imu-count',
        type=int,
        default=0,
        help='Message count hint for /imu/data topic metadata (default: 0)',
    )
    return parser.parse_args()


def build_client() -> tuple[DataSetsApi, MetaDataApi]:
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        raise RuntimeError('unknown active instance key in rdpms_cli config')

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    Configuration.set_default(api_conf)
    ApiClient.set_default(ApiClient(api_conf))
    client = ApiClient.get_default()

    return DataSetsApi(client), MetaDataApi(client)


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


def build_time_series_metadata(*, gnss_count: int, imu_count: int) -> dict[str, object]:
    now_iso = dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds')
    return {
        'topics': [
            {
                'name': GNSS_TOPIC,
                'metadata': {
                    'messageCount': max(gnss_count, 0),
                    'firstMessageTimestamp': now_iso,
                    'lastMessageTimestamp': now_iso,
                },
                'messageType': {
                    'name': GNSS_TYPE,
                    'description': 'ROS2 GNSS topic payload',
                    'reference': 'https://docs.ros2.org/latest/api/sensor_msgs/msg/NavSatFix.html',
                    'fields': [
                        {'name': 'header', 'type': {'descriptor': 'ROS std_msgs/Header', 'type': 'object'}},
                        {'name': 'status', 'type': {'descriptor': 'GNSS status', 'type': 'object'}},
                        {'name': 'latitude', 'type': {'descriptor': 'Latitude in degrees', 'type': 'float64'}},
                        {'name': 'longitude', 'type': {'descriptor': 'Longitude in degrees', 'type': 'float64'}},
                        {'name': 'altitude', 'type': {'descriptor': 'Altitude in meters', 'type': 'float64'}},
                    ],
                },
            },
            {
                'name': IMU_TOPIC,
                'metadata': {
                    'messageCount': max(imu_count, 0),
                    'firstMessageTimestamp': now_iso,
                    'lastMessageTimestamp': now_iso,
                },
                'messageType': {
                    'name': IMU_TYPE,
                    'description': 'ROS2 IMU topic payload',
                    'reference': 'https://docs.ros2.org/latest/api/sensor_msgs/msg/Imu.html',
                    'fields': [
                        {'name': 'header', 'type': {'descriptor': 'ROS std_msgs/Header', 'type': 'object'}},
                        {'name': 'orientation', 'type': {'descriptor': 'Quaternion orientation', 'type': 'object'}},
                        {
                            'name': 'angular_velocity',
                            'type': {'descriptor': 'Angular velocity in rad/s', 'type': 'object'},
                        },
                        {
                            'name': 'linear_acceleration',
                            'type': {'descriptor': 'Linear acceleration in m/s^2', 'type': 'object'},
                        },
                    ],
                },
            },
        ]
    }


def main() -> int:
    args = parse_args()

    if args.gnss_count < 0 or args.imu_count < 0:
        raise RuntimeError('--gnss-count and --imu-count must be >= 0')

    ds_api, meta_api = build_client()

    dataset_id = uuid.UUID(args.dataset)
    metadata_doc = build_time_series_metadata(gnss_count=args.gnss_count, imu_count=args.imu_count)
    metadata_body = json.dumps(metadata_doc).encode('utf-8')

    metadata_result = ds_api.api_v1_data_datasets_id_metadata_key_put(
        dataset_id,
        args.metadata_key,
        body=metadata_body,
        _content_type='application/octet-stream',
    )
    metadata_id = metadata_id_to_uuid(metadata_result)
    print(f'[info] assigned metadata id {metadata_id} to dataset {dataset_id} with key {args.metadata_key}')

    schema_guid = find_schema_guid_by_urn(meta_api, args.schema_urn)
    if not schema_guid:
        print(f'[warn] schema URN not found, metadata was not validated: {args.schema_urn}')
        return 0

    is_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(metadata_id, schema_guid)
    if not is_valid:
        print(f'[warn] metadata {metadata_id} did not validate against schema {schema_guid}')
        return 2

    print(f'[ok] metadata {metadata_id} validated against schema {schema_guid}')
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
