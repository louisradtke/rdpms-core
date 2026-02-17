#!/usr/bin/env python3
"""Debug helper: generate IMU CSV example data and upload it as a dataset."""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import hashlib
import json
import math
import re
import traceback
from pathlib import Path
import random
import sys
import uuid

import requests

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[2]
CLI_SRC_ROOT = REPO_ROOT / "rdpms-cli"
if str(CLI_SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(CLI_SRC_ROOT))

from rdpms_cli.openapi_client.api_client import ApiClient
from rdpms_cli.openapi_client.configuration import Configuration
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.openapi_client.api.files_api import FilesApi
from rdpms_cli.openapi_client.api.meta_data_api import MetaDataApi
from rdpms_cli.openapi_client.models.data_set_create_request_dto import DataSetCreateRequestDTO
from rdpms_cli.openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.util.TypeStore import get_types
from rdpms_cli.util.config_store import load_file

TSDATA_SCHEMA_URN = 'urn:rdpms:core:schema:time-series-container:v1'
TSDATA_KEY = 'rdpms.tsdata'
IMU_HEADERS = ['seconds', 'acc_x', 'acc_y', 'acc_z', 'roll_rate', 'pitch_rate', 'yaw_rate']


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Generate a debug IMU CSV file and upload it through rdpms_cli API client "
            "to a given collection."
        )
    )
    parser.add_argument("--collection", "-c", required=True, help="Target collection id")
    parser.add_argument("--name", help="Dataset name (default: generated from timestamp)")
    parser.add_argument(
        "--rows",
        type=int,
        default=500,
        help="Number of IMU rows to generate (default: 500)",
    )
    parser.add_argument(
        "--freq-hz",
        type=float,
        default=100.0,
        help="Sample frequency in Hz (default: 100.0)",
    )
    parser.add_argument(
        "--output-dir",
        default="/tmp",
        help="Directory where the CSV file will be created (default: /tmp)",
    )
    parser.add_argument(
        "--seed",
        type=int,
        default=7,
        help="Random seed for reproducible sample data (default: 7)",
    )
    parser.add_argument(
        "--keep-file",
        action="store_true",
        help="Keep generated CSV file after upload (default: delete it)",
    )
    return parser.parse_args()


def slugify(value: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")
    return slug or "debug-imu-dataset"


def generate_imu_csv(path: Path, rows: int, freq_hz: float, seed: int) -> None:
    if rows <= 0:
        raise ValueError("--rows must be > 0")
    if freq_hz <= 0:
        raise ValueError("--freq-hz must be > 0")

    rng = random.Random(seed)
    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(IMU_HEADERS)

        for i in range(rows):
            t = i / freq_hz

            acc_x = 0.35 * math.sin(2.0 * math.pi * 0.8 * t) + rng.uniform(-0.03, 0.03)
            acc_y = 0.42 * math.sin(2.0 * math.pi * 1.2 * t + 0.7) + rng.uniform(-0.03, 0.03)
            acc_z = 9.81 + 0.18 * math.sin(2.0 * math.pi * 0.25 * t) + rng.uniform(-0.02, 0.02)

            roll_rate = 0.16 * math.sin(2.0 * math.pi * 0.4 * t) + rng.uniform(-0.01, 0.01)
            pitch_rate = 0.11 * math.sin(2.0 * math.pi * 0.5 * t + 0.5) + rng.uniform(-0.01, 0.01)
            yaw_rate = 0.13 * math.sin(2.0 * math.pi * 0.3 * t + 1.0) + rng.uniform(-0.01, 0.01)

            writer.writerow(
                [
                    f"{t:.6f}",
                    f"{acc_x:.6f}",
                    f"{acc_y:.6f}",
                    f"{acc_z:.6f}",
                    f"{roll_rate:.6f}",
                    f"{pitch_rate:.6f}",
                    f"{yaw_rate:.6f}",
                ]
            )


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


def create_dataset_id(ds_api: DataSetsApi, ds_req: DataSetCreateRequestDTO) -> uuid.UUID:
    created = ds_api.api_v1_data_datasets_new_post(ds_req)
    dataset_id = getattr(created, 'id', None)
    if not dataset_id:
        raise RuntimeError('dataset creation returned no id')
    return uuid.UUID(str(dataset_id))


def build_time_series_container_metadata(dataset_name: str, csv_name: str, row_count: int) -> dict[str, object]:
    return {
        'topics': [
            {
                'name': f'/imu',
                'metadata': {'messageCount': row_count},
                'messageType': {
                    'name': 'rdpms.debug.imu.csv',
                    'description': f'Multi-dimensional IMU time series extracted from {csv_name}',
                    'reference': 'urn:rdpms:debug:imu-csv:v1',
                    'fields': [
                        {
                            'name': header,
                            'type': {
                                'descriptor': f"CSV column '{header}'",
                                'type': 'number',
                            },
                        }
                        for header in IMU_HEADERS
                    ],
                },
            }
        ],
    }


def upload_generated_file(csv_path: Path, collection_id: str, dataset_name: str, row_count: int) -> tuple[str, str]:
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        raise RuntimeError('unknown active instance key in rdpms_cli config')

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    Configuration.set_default(api_conf)
    ApiClient.set_default(ApiClient(api_conf))
    client = ApiClient.get_default()

    ds_api = DataSetsApi()
    files_api = FilesApi()
    meta_api = MetaDataApi()

    now_utc = dt.datetime.now(dt.timezone.utc)
    ds_req = DataSetCreateRequestDTO(
        name=dataset_name,
        slug=slugify(dataset_name),
        created_stamp_utc=now_utc,
        collection_id=collection_id,
    )

    ds_id = create_dataset_id(ds_api, ds_req)

    file_stats = csv_path.stat()
    sha256 = hashlib.sha256()
    with csv_path.open("rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
            sha256.update(chunk)

    type_store = get_types("", client)
    content_type = type_store.resolve_by_ending(csv_path.name)

    upload_req = S3FileCreateRequestDTO(
        name=csv_path.name,
        size_bytes=file_stats.st_size,
        plain_sha256_hash=sha256.hexdigest(),
        created_stamp=dt.datetime.fromtimestamp(file_stats.st_ctime, dt.timezone.utc),
        content_type_id=content_type.id,
    )

    upload_resp = ds_api.api_v1_data_datasets_id_add_s3_post(ds_id, upload_req)
    with csv_path.open("rb") as f:
        response = requests.put(upload_resp.upload_uri, data=f)

    response.raise_for_status()

    uploaded_file_id = upload_resp.file_id
    if not uploaded_file_id:
        raise RuntimeError('upload response did not include file id')

    ds_api.api_v1_data_datasets_id_seal_put(ds_id)

    schema_guid = find_schema_guid_by_urn(meta_api, TSDATA_SCHEMA_URN)
    if not schema_guid:
        print(f'[warn] schema URN not found, metadata will not be validated: {TSDATA_SCHEMA_URN}')

    metadata_doc = build_time_series_container_metadata(dataset_name, csv_path.name, row_count)
    metadata_body = json.dumps(metadata_doc).encode('utf-8')

    ds_meta = ds_api.api_v1_data_datasets_id_metadata_key_put(
        ds_id,
        TSDATA_KEY,
        body=metadata_body,
        _content_type='application/octet-stream',
    )
    dataset_meta_id = metadata_id_to_uuid(ds_meta)

    file_meta = files_api.api_v1_data_files_id_metadata_key_put(
        uploaded_file_id,
        TSDATA_KEY,
        body=metadata_body,
        _content_type='application/octet-stream',
    )
    file_meta_id = metadata_id_to_uuid(file_meta)

    if schema_guid:
        dataset_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(dataset_meta_id, schema_guid)
        if not dataset_valid:
            print(f'[warn] dataset metadata {dataset_meta_id} did not validate against schema {schema_guid}')
        file_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(file_meta_id, schema_guid)
        if not file_valid:
            print(f'[warn] file metadata {file_meta_id} did not validate against schema {schema_guid}')

    return str(ds_id), str(uploaded_file_id)


def main() -> int:
    args = parse_args()

    timestamp_utc = dt.datetime.now(dt.timezone.utc).strftime("%Y%m%dT%H%M%SZ")
    dataset_name = args.name or f"debug-imu-{timestamp_utc}"

    out_dir = Path(args.output_dir)
    out_dir.mkdir(parents=True, exist_ok=True)
    csv_path = out_dir / f"{slugify(dataset_name)}.csv"

    print(f"[debug imu upload] generating CSV: {csv_path}")
    generate_imu_csv(
        path=csv_path,
        rows=args.rows,
        freq_hz=args.freq_hz,
        seed=args.seed,
    )

    print(
        f"[debug imu upload] uploading dataset '{dataset_name}' to collection '{args.collection}'"
    )

    upload_ok = False
    try:
        dataset_id, file_id = upload_generated_file(
            csv_path=csv_path,
            collection_id=args.collection,
            dataset_name=dataset_name,
            row_count=args.rows,
        )
        upload_ok = True
    except ApiException as exc:
        print(f"[debug imu upload] API error ({exc.status}): {exc.reason}")
        if exc.body:
            print(exc.body)
        print(traceback.format_exc())
        return 1
    except Exception as exc:
        print(f"[debug imu upload] failed: {exc}")
        print(traceback.format_exc())
        return 1
    finally:
        if upload_ok and not args.keep_file and csv_path.exists():
            csv_path.unlink()

    print(f"[debug imu upload] success; dataset id: {dataset_id}")
    print(f"[debug imu upload] success; file id: {file_id}")
    if args.keep_file:
        print(f"[debug imu upload] kept CSV at: {csv_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
