#!/usr/bin/env python3
"""Debug helper: generate IMU CSV example data and upload it as a dataset."""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import hashlib
import math
import re
from pathlib import Path
import random
import sys

import requests

# Allow direct execution from repository root without requiring editable install.
REPO_ROOT = Path(__file__).resolve().parents[2]
CLI_SRC_ROOT = REPO_ROOT / "rdpms-cli"
if str(CLI_SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(CLI_SRC_ROOT))

from rdpms_cli.openapi_client.api_client import ApiClient
from rdpms_cli.openapi_client.configuration import Configuration
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.openapi_client.models.data_set_summary_dto import DataSetSummaryDTO
from rdpms_cli.openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.util.TypeStore import get_types
from rdpms_cli.util.config_store import load_file


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
    headers = [
        "seconds",
        "acc_x",
        "acc_y",
        "acc_z",
        "roll_rate",
        "pitch_rate",
        "yaw_rate",
    ]

    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(headers)

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


def upload_generated_file(csv_path: Path, collection_id: str, dataset_name: str) -> str:
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        raise RuntimeError("unknown active instance key in rdpms_cli config")

    instance = conf.instances[conf.active_instance_key]
    client = ApiClient(Configuration(host=instance.base_url))
    ds_api = DataSetsApi(client)

    now_utc = dt.datetime.now(dt.timezone.utc)
    ds_req = DataSetSummaryDTO(
        name=dataset_name,
        slug=slugify(dataset_name),
        created_stamp_utc=now_utc,
        collection_id=collection_id,
    )

    ds_detailed = ds_api.api_v1_data_datasets_post(ds_req)
    ds_id = ds_detailed.id

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
    ds_api.api_v1_data_datasets_id_seal_put(ds_id)
    return ds_id


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
        dataset_id = upload_generated_file(
            csv_path=csv_path,
            collection_id=args.collection,
            dataset_name=dataset_name,
        )
        upload_ok = True
    except ApiException as exc:
        print(f"[debug imu upload] API error ({exc.status}): {exc.reason}")
        if exc.body:
            print(exc.body)
        return 1
    except Exception as exc:
        print(f"[debug imu upload] failed: {exc}")
        return 1
    finally:
        if upload_ok and not args.keep_file and csv_path.exists():
            csv_path.unlink()

    print(f"[debug imu upload] success; dataset id: {dataset_id}")
    if args.keep_file:
        print(f"[debug imu upload] kept CSV at: {csv_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
