#!/usr/bin/env python3
"""Debug helper: process CSV datasets from one collection and attach visualization metadata."""

from __future__ import annotations

import argparse
import csv
import datetime as dt
import hashlib
import io
import json
import re
import sys
import tempfile
import traceback
import uuid
from pathlib import Path

import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
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
from rdpms_cli.openapi_client.models.data_set_summary_dto import DataSetSummaryDTO
from rdpms_cli.openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
from rdpms_cli.openapi_client.exceptions import ApiException
from rdpms_cli.util.TypeStore import get_types
from rdpms_cli.util.config_store import load_file

VISUALIZATION_SCHEMA_URN = "urn:rdpms:core:schema:visualization-manifest:v1"
TRACKER_FILENAME = "processed_source_datasets.csv"
EXPECTED_HEADERS = ["seconds", "acc_x", "acc_y", "acc_z", "roll_rate", "pitch_rate", "yaw_rate"]


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Process source collection CSV datasets, generate matplotlib PNGs, upload them "
            "to target collection, and assign visualization metadata to source datasets."
        )
    )
    parser.add_argument("--source-collection", "-s", required=True, help="Source collection id")
    parser.add_argument("--target-collection", "-t", required=True, help="Target collection id")
    parser.add_argument(
        "--metadata-key",
        default="viz",
        help="Metadata key to assign on source dataset (default: viz)",
    )
    parser.add_argument(
        "--schema-urn",
        default=VISUALIZATION_SCHEMA_URN,
        help="Schema URN used for validation after metadata assignment",
    )
    parser.add_argument(
        "--force",
        action="store_true",
        help="Process datasets even if tracker has status=success",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Optional max number of source datasets to process this run (0 = unlimited)",
    )
    return parser.parse_args()


def slugify(value: str) -> str:
    slug = re.sub(r"[^a-z0-9]+", "-", value.lower()).strip("-")
    return slug or "debug-visualization"


def build_client() -> tuple[ApiClient, DataSetsApi, MetaDataApi]:
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        raise RuntimeError("unknown active instance key in rdpms_cli config")

    instance = conf.instances[conf.active_instance_key]
    client = ApiClient(Configuration(host=instance.base_url))
    return client, DataSetsApi(client), MetaDataApi(client)


def tracker_path() -> Path:
    return Path(__file__).resolve().parent / TRACKER_FILENAME


def ensure_tracker_header(path: Path) -> None:
    if path.exists():
        return

    with path.open("w", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(
            [
                "processed_at_utc",
                "status",
                "source_dataset_id",
                "source_dataset_name",
                "source_csv_file_id",
                "target_dataset_id",
                "target_plot_file_id",
                "message",
            ]
        )


def load_successful_source_ids(path: Path) -> set[str]:
    if not path.exists():
        return set()

    success_ids: set[str] = set()
    with path.open("r", newline="", encoding="utf-8") as f:
        reader = csv.DictReader(f)
        for row in reader:
            if (row.get("status") or "").strip().lower() != "success":
                continue
            source_id = (row.get("source_dataset_id") or "").strip()
            if source_id:
                success_ids.add(source_id)
    return success_ids


def append_tracker_row(
    path: Path,
    *,
    status: str,
    source_dataset_id: str,
    source_dataset_name: str,
    source_csv_file_id: str,
    target_dataset_id: str,
    target_plot_file_id: str,
    message: str,
) -> None:
    stamp = dt.datetime.now(dt.timezone.utc).isoformat(timespec="seconds")
    with path.open("a", newline="", encoding="utf-8") as f:
        writer = csv.writer(f)
        writer.writerow(
            [
                stamp,
                status,
                source_dataset_id,
                source_dataset_name,
                source_csv_file_id,
                target_dataset_id,
                target_plot_file_id,
                message,
            ]
        )


def find_csv_file(dataset_detailed) -> object:
    files = dataset_detailed.files or []
    csv_files = [
        f
        for f in files
        if (f.name and f.name.lower().endswith(".csv"))
        or ((f.content_type and (f.content_type.abbreviation or "").lower() == "csv"))
        or ((f.content_type and (f.content_type.mime_type or "").lower() in {"text/csv", "application/csv"}))
    ]
    if not csv_files:
        raise ValueError("no CSV file found in dataset")
    return csv_files[0]


def parse_imu_csv(blob: bytes) -> dict[str, list[float]]:
    text = blob.decode("utf-8")
    reader = csv.DictReader(io.StringIO(text))
    if not reader.fieldnames:
        raise ValueError("CSV has no header")

    normalized_headers = [h.strip() for h in reader.fieldnames]
    if normalized_headers != EXPECTED_HEADERS:
        raise ValueError(
            "CSV header mismatch: expected "
            f"{EXPECTED_HEADERS}, got {normalized_headers}"
        )

    data: dict[str, list[float]] = {key: [] for key in EXPECTED_HEADERS}
    for idx, row in enumerate(reader, start=2):
        try:
            for key in EXPECTED_HEADERS:
                data[key].append(float((row.get(key) or "").strip()))
        except ValueError as exc:
            raise ValueError(f"CSV parse error in row {idx}: {exc}") from exc

    if not data["seconds"]:
        raise ValueError("CSV contains no data rows")

    return data


def create_plot_png(data: dict[str, list[float]], output_path: Path) -> None:
    fig, axes = plt.subplots(2, 1, figsize=(12, 8), sharex=True)

    t = data["seconds"]
    axes[0].plot(t, data["acc_x"], label="acc_x", linewidth=1.2)
    axes[0].plot(t, data["acc_y"], label="acc_y", linewidth=1.2)
    axes[0].plot(t, data["acc_z"], label="acc_z", linewidth=1.2)
    axes[0].set_title("IMU acceleration")
    axes[0].set_ylabel("m/s^2")
    axes[0].grid(True, alpha=0.3)
    axes[0].legend(loc="upper right")

    axes[1].plot(t, data["roll_rate"], label="roll_rate", linewidth=1.2)
    axes[1].plot(t, data["pitch_rate"], label="pitch_rate", linewidth=1.2)
    axes[1].plot(t, data["yaw_rate"], label="yaw_rate", linewidth=1.2)
    axes[1].set_title("IMU angular rates")
    axes[1].set_xlabel("seconds")
    axes[1].set_ylabel("rad/s")
    axes[1].grid(True, alpha=0.3)
    axes[1].legend(loc="upper right")

    fig.tight_layout()
    fig.savefig(output_path, dpi=180)
    plt.close(fig)


def upload_file_to_dataset(ds_api: DataSetsApi, types, dataset_id: str, file_path: Path) -> str:
    stats = file_path.stat()
    sha256 = hashlib.sha256()
    with file_path.open("rb") as f:
        for chunk in iter(lambda: f.read(65536), b""):
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
    with file_path.open("rb") as f:
        response = requests.put(upload_resp.upload_uri, data=f)
    response.raise_for_status()

    refreshed = ds_api.api_v1_data_datasets_id_get(dataset_id)
    uploaded = next((f for f in (refreshed.files or []) if f.name == file_path.name), None)
    if not uploaded or not uploaded.id:
        raise RuntimeError("uploaded file not found in dataset after upload")
    return str(uploaded.id)


def create_target_dataset(ds_api: DataSetsApi, target_collection_id: uuid.UUID, name: str) -> str:
    now_utc = dt.datetime.now(dt.timezone.utc)
    request = DataSetSummaryDTO(
        name=name,
        slug=f'{slugify(name)}-{now_utc.strftime("%Y%m%d%H%M%S")}',
        created_stamp_utc=now_utc,
        collection_id=target_collection_id,
    )
    created = ds_api.api_v1_data_datasets_post(request)
    if not created.id:
        raise RuntimeError("target dataset creation returned no id")
    return str(created.id)


def find_schema_guid_by_urn(meta_api: MetaDataApi, schema_urn: str) -> uuid.UUID | None:
    schemas = meta_api.api_v1_data_schemas_get()
    for schema in schemas:
        if (schema.schema_id or "").strip().lower() == schema_urn.strip().lower() and schema.id:
            return schema.id
    return None


def assign_visualization_metadata(
    ds_api: DataSetsApi,
    meta_api: MetaDataApi,
    source_dataset_id: str,
    metadata_key: str,
    source_dataset_name: str,
    plot_file_id: str,
    schema_guid: uuid.UUID | None,
) -> str:
    manifest = {
        "id": str(uuid.uuid4()),
        "title": f"Visualization for {source_dataset_name}",
        "views": [
            {
                "title": "Generated visualizations",
                "items": [
                    {
                        "title": "IMU plot",
                        "source": {"fileId": plot_file_id},
                        "renderer": {"kind": ["rdpms.image"], "default": "rdpms.image"},
                        "collapsible": False,
                        "collapsedByDefault": False,
                    }
                ],
            }
        ],
    }

    body = json.dumps(manifest).encode("utf-8")
    metadata_result = ds_api.api_v1_data_datasets_id_metadata_key_put(
        source_dataset_id,
        metadata_key,
        body=body,
        _content_type="application/octet-stream",
    )
    if hasattr(metadata_result, "id"):
        metadata_value = getattr(metadata_result, "id")
    else:
        metadata_value = metadata_result
    if metadata_value is None:
        raise RuntimeError("metadata assignment returned no metadata id")
    metadata_uuid = uuid.UUID(str(metadata_value))

    if schema_guid:
        is_valid = meta_api.api_v1_data_metadata_id_validate_schema_id_put(metadata_uuid, schema_guid)
        if not is_valid:
            print(
                f"[warn] metadata {metadata_uuid} for source dataset {source_dataset_id} did not validate "
                f"against schema {schema_guid}"
            )

    return str(metadata_uuid)


def process_source_dataset(
    source_dataset,
    ds_api: DataSetsApi,
    meta_api: MetaDataApi,
    types,
    target_collection_id: uuid.UUID,
    metadata_key: str,
    schema_guid: uuid.UUID | None,
) -> tuple[str, str, str, str, str]:
    source_dataset_id = str(source_dataset.id)
    source_name = source_dataset.name or source_dataset_id

    detailed = ds_api.api_v1_data_datasets_id_get(source_dataset_id)
    csv_file = find_csv_file(detailed)
    if not csv_file.id:
        raise RuntimeError("source csv file has no id")

    download_uri = csv_file.download_uri
    if not download_uri:
        files_api = FilesApi(ds_api.api_client)
        file_summary = files_api.api_v1_data_files_id_get(csv_file.id)
        download_uri = file_summary.download_uri
    if not download_uri:
        raise RuntimeError("file has no download URI")

    response = requests.get(download_uri, allow_redirects=True, timeout=120)
    response.raise_for_status()
    raw_bytes = response.content

    imu_data = parse_imu_csv(raw_bytes)

    with tempfile.TemporaryDirectory(prefix="rdpms-debug-viz-") as tmp_dir:
        png_path = Path(tmp_dir) / f"{slugify(source_name)}-plot.png"
        create_plot_png(imu_data, png_path)

        target_dataset_name = f"viz-{source_name}-{source_dataset_id[:8]}"
        target_dataset_id = create_target_dataset(ds_api, target_collection_id, target_dataset_name)
        target_plot_file_id = upload_file_to_dataset(ds_api, types, target_dataset_id, png_path)
        ds_api.api_v1_data_datasets_id_seal_put(target_dataset_id)

    assign_visualization_metadata(
        ds_api=ds_api,
        meta_api=meta_api,
        source_dataset_id=source_dataset_id,
        metadata_key=metadata_key,
        source_dataset_name=source_name,
        plot_file_id=target_plot_file_id,
        schema_guid=schema_guid,
    )

    return source_dataset_id, source_name, str(csv_file.id), target_dataset_id, target_plot_file_id


def main() -> int:
    args = parse_args()

    source_collection_id = uuid.UUID(args.source_collection)
    target_collection_id = uuid.UUID(args.target_collection)

    tracker = tracker_path()
    ensure_tracker_header(tracker)
    processed_success = load_successful_source_ids(tracker)

    client, ds_api, meta_api = build_client()
    types = get_types("", client)

    schema_guid = find_schema_guid_by_urn(meta_api, args.schema_urn)
    if schema_guid:
        print(f"[info] resolved schema GUID for {args.schema_urn}: {schema_guid}")
    else:
        print(f"[warn] schema URN not found, metadata will not be validated: {args.schema_urn}")

    source_datasets = ds_api.api_v1_data_datasets_get(collection_id=source_collection_id)
    print(f"[info] found {len(source_datasets)} datasets in source collection {source_collection_id}")

    processed_count = 0
    success_count = 0
    skipped_count = 0

    for source_dataset in source_datasets:
        source_dataset_id = str(source_dataset.id)
        source_dataset_name = source_dataset.name or source_dataset_id

        if not args.force and source_dataset_id in processed_success:
            skipped_count += 1
            print(f"[skip] already processed successfully: {source_dataset_name} ({source_dataset_id})")
            continue

        if args.limit > 0 and processed_count >= args.limit:
            break

        processed_count += 1
        print(f"[process] source dataset: {source_dataset_name} ({source_dataset_id})")

        try:
            source_id, source_name, source_csv_id, target_ds_id, target_plot_id = process_source_dataset(
                source_dataset=source_dataset,
                ds_api=ds_api,
                meta_api=meta_api,
                types=types,
                target_collection_id=target_collection_id,
                metadata_key=args.metadata_key,
                schema_guid=schema_guid,
            )

            append_tracker_row(
                tracker,
                status="success",
                source_dataset_id=source_id,
                source_dataset_name=source_name,
                source_csv_file_id=source_csv_id,
                target_dataset_id=target_ds_id,
                target_plot_file_id=target_plot_id,
                message="",
            )
            success_count += 1
            print(
                f"[ok] source={source_id} csv={source_csv_id} -> target_dataset={target_ds_id}, "
                f"plot_file={target_plot_id}"
            )
        except Exception as exc:
            append_tracker_row(
                tracker,
                status="failed",
                source_dataset_id=source_dataset_id,
                source_dataset_name=source_dataset_name,
                source_csv_file_id="",
                target_dataset_id="",
                target_plot_file_id="",
                message=str(exc).strip(),
            )
            print(f"[error] failed for dataset {source_dataset_id}: {exc}")
            print(traceback.format_exc())

    print(
        "[summary] "
        f"processed={processed_count}, success={success_count}, skipped_already_processed={skipped_count}, "
        f"tracker={tracker}"
    )
    print(
        "[note] current web UI resolves visualization fileIds from the source dataset file list. "
        "If plot files are uploaded to another collection/dataset, they may not render in the current UI."
    )
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except ApiException as exc:
        print(f"[fatal] API error ({exc.status}): {exc.reason}")
        if exc.body:
            print(exc.body)
        raise SystemExit(1)
