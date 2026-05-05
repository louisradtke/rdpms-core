#!/usr/bin/env python3
"""Annotate rosbag datasets with minimal rdpms.tsdata metadata."""

from __future__ import annotations

import argparse
import datetime as dt
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
    detect_rosbag_input,
    download_dataset_files,
    ensure_tracker_header,
    find_schema_guid_by_urn,
    get_dataset_details,
    list_collection_datasets,
    load_successful_source_ids,
    summarize_time_series_topics,
)
from common_develop_tooling import add_develop_directory_options, temporary_directory
from rdpms_cli.openapi_client.exceptions import ApiException

TSDATA_KEY = 'rdpms.tsdata'
TSDATA_SCHEMA_URN = 'urn:rdpms:core:schema:time-series-container:v1'
TRACKER_FILENAME = 'cache/annotated_rosbag_tsdata.csv'


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description='Annotate rosbag datasets with minimal time-series metadata.')
    selector = parser.add_mutually_exclusive_group(required=True)
    selector.add_argument('--dataset', '-d', help='Single dataset id to annotate')
    selector.add_argument('--source-collection', '-s', help='Collection id to scan for rosbag datasets')
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
    parser.add_argument('--tracker-id', help='Optional tracker id to isolate processed-state for this workflow')
    parser.add_argument('--force', action='store_true', help='Re-annotate datasets even if tracker says success')
    parser.add_argument('--limit', type=int, default=0, help='Optional max number of datasets to process')
    add_develop_directory_options(parser)
    return parser.parse_args()


def tracker_path(tracker_id: str | None, cache_dir: str | None) -> Path:
    return build_tracker_path(Path(__file__), TRACKER_FILENAME, tracker_id, cache_dir)


def process_dataset(dataset_id: uuid.UUID, *, ds_api, files_api, meta_api, args: argparse.Namespace, schema_guid):
    dataset = get_dataset_details(ds_api, dataset_id)
    dataset_name = str(dataset.name or dataset_id)

    with temporary_directory('rdpms-annotate-rosbag-', args.tmp_download_base_dir) as tmp:
        downloaded = download_dataset_files(dataset, files_api, tmp)
        bag_input = detect_rosbag_input(downloaded)
        topic_summary = summarize_time_series_topics(bag_input)
        metadata_doc = build_minimal_time_series_metadata(topic_summary)
        metadata_id = assign_time_series_metadata(
            ds_api,
            meta_api,
            dataset_id=dataset_id,
            metadata_key=args.metadata_key,
            metadata_doc=metadata_doc,
            schema_guid=schema_guid,
        )
    return dataset_name, metadata_id, len(metadata_doc['topics'])


def main() -> int:
    args = parse_args()
    tracker = tracker_path(args.tracker_id, args.cache_dir)
    ensure_tracker_header(
        tracker,
        ['processed_at_utc', 'status', 'source_dataset_id', 'source_dataset_name', 'metadata_id', 'message'],
    )
    processed_success = load_successful_source_ids(tracker)

    _client, ds_api, files_api, meta_api = build_client()
    schema_guid = find_schema_guid_by_urn(meta_api, args.schema_urn)
    if schema_guid:
        print(f'[info] resolved schema GUID for {args.schema_urn}: {schema_guid}')
    else:
        print(f'[warn] schema URN not found, metadata will not be validated: {args.schema_urn}')

    if args.dataset:
        dataset_ids = [str(uuid.UUID(args.dataset))]
    else:
        source_collection_id = uuid.UUID(args.source_collection)
        source_datasets = list_collection_datasets(ds_api, source_collection_id)
        dataset_ids = [str(ds.id) for ds in source_datasets if ds.id]
        print(f'[info] found {len(dataset_ids)} datasets in source collection {source_collection_id}')

    processed_count = 0
    success_count = 0
    skipped_count = 0

    for dataset_id_str in dataset_ids:
        if not args.force and args.source_collection and dataset_id_str in processed_success:
            skipped_count += 1
            print(f'[skip] already annotated successfully: {dataset_id_str}')
            continue

        if args.limit > 0 and processed_count >= args.limit:
            break

        dataset_id = uuid.UUID(dataset_id_str)
        processed_count += 1
        try:
            dataset_name, metadata_id, topic_count = process_dataset(
                dataset_id,
                ds_api=ds_api,
                files_api=files_api,
                meta_api=meta_api,
                args=args,
                schema_guid=schema_guid,
            )
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'success',
                    str(dataset_id),
                    dataset_name,
                    str(metadata_id),
                    '',
                ],
            )
            success_count += 1
            print(f'[ok] annotated {dataset_name} ({dataset_id}) with {topic_count} topic entries')
        except Exception as exc:
            append_tracker_row(
                tracker,
                [
                    dt.datetime.now(dt.timezone.utc).isoformat(timespec='seconds'),
                    'failed',
                    str(dataset_id),
                    '',
                    '',
                    str(exc).strip(),
                ],
            )
            print(f'[error] failed for dataset {dataset_id}: {exc}')
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
