# Develop Tool Workflows

This directory currently contains prototype and debug-oriented tool-scripts used to exercise RDPMS end-to-end workflow concepts during development.

The scripts are plain Python entry points and currently combine:
- dataset discovery and selection,
- API interaction through the generated `rdpms-cli` OpenAPI client,
- instance/config loading and content-type resolution via `rdpms-cli` utilities,
- tool-specific processing logic,
- registration of derived artifacts and metadata.

## Workflows

Workflows are a composition of tools.

### Overview

#### 1. Linear Workflow

1. Raw data gets uploaded to a collection
2. Metadata gets created for the raw data
3. This raw data gets processed into an interim product (examples: truncation, compression and reordering of a Rosbag)
   - Metadata gets created for the interim product on the fly
4. Visualization gets created from the interim product
   - The source dataset ("interim product") gets annotated with visualization metadata

#### 2. Simple DAG-Workflow

1. Raw data gets uploaded to a collection
2. Metadata gets created for the raw data
3. The raw data fans out into multiple independent processing steps
   - Example branches: CSV extraction, bag reduction, quick-look visualization
   - Each branch may produce its own derived dataset and metadata
4. Downstream tools consume branch outputs independently
   - Some branches may terminate in a visualization
   - Other branches may continue into additional processing stages
5. The workflow is therefore no longer a single chain, but a small dependency graph
   - Several derived datasets can reference the same raw source dataset
   - Provenance should remain traceable across all branches

#### 3. Join-Workflows

1. Multiple source workflows run independently at first
   - Example: GNSS/IMU CSV extraction, image preparation, and bag cleanup
2. A join step consumes outputs from two or more upstream workflows
   - This requires a join criterion such as shared provenance, timestamps, metadata, or explicit linkage
3. The join step optionally produces a combined derived product

### 4. Workflow with arbitrary number of outputs

todo: This is a multi-fork, where each branch produces a derived dataset and metadata. conditionally (metadata is condition), a new pipeline is executed.

Outline:

1. One workflow step can produce an arbitrary number of outputs
   - Each output branch may carry its own metadata and derived datasets
2. Branches can trigger additional workflows conditionally
   - The condition is expressed through the assigned metadata

## Tools

### Overview

| Kind   | Name | Entry point | Trigger model | Notes |
|--------| --- | --- | --- | --- |
| Tool   | Upload IMU CSV | `upload_debug_imu_csv/upload_debug_imu_csv.py` | Manual trigger | Generates synthetic IMU CSV data, uploads it as a dataset, and assigns `rdpms.tsdata`. Useful as a source producer for the CSV visualization workflow. |
| Tool   | Process Collection CSV to Viz | `process_collection_csv_to_viz/process_collection_csv_to_viz.py` | Cyclic / repeated run | Iterates datasets in a source collection, generates PNG plots from CSV input, uploads the plot to a target collection, and assigns `rdpms.viz` on the source dataset. Uses `cache/processed_source_datasets.csv` as an idempotence tracker. |
| Tool   | Extract Rosbag GNSS + IMU to CSV | `extract_rosbag_gnss_imu_to_csv/extract_rosbag_gnss_imu_to_csv.py` | Cyclic / repeated run | Filters source datasets by `rdpms.tsdata`, downloads ROS2 bag files, extracts `/gnss` and `/imu/data` to CSV, uploads the derived CSV files to a target collection, refreshes `rdpms.tsdata`, and assigns `rdpms.viz` on the source dataset. Uses `cache/processed_rosbag_source_datasets.csv` as an idempotence tracker. |
| Helper | Annotate Dataset Time Series Metadata | `annotate_dataset_time_series_metadata/annotate_dataset_time_series_metadata.py` | Manual trigger | Assigns synthetic/manual `rdpms.tsdata` metadata to a dataset and validates it against the time-series schema. |
| Tool   | Annotate Rosbag TSData | `annotate_rosbag_tsdata/annotate_rosbag_tsdata.py` | Manual trigger or cyclic / repeated run | Downloads rosbag dataset files, summarizes all contained topics, and assigns minimal `rdpms.tsdata` metadata with topic name, message count, first/last timestamp, and `messageType.name`. |
| Tool   | Register Existing S3 Rosbags | `register_existing_s3_rosbags/register_existing_s3_rosbags.py` | Manual trigger | Recursively scans an S3 prefix for rosbag directories identified by `metadata.yaml`, skips already registered dataset slugs, and registers each bag as a sealed S3-backed dataset through the API. |
| Tool   | Trim Motion Rosbag | `trim_motion_rosbag/trim_motion_rosbag.py` | Cyclic / repeated run | Queries source datasets by `rdpms.tsdata` for a configured topic/type, detects a movement window from a `Float32` topic, rewrites a new uncompressed bag by record timestamp and configured topics, uploads the result dataset to a target collection, and assigns minimal `rdpms.tsdata` on the derived bag. |
| Tool   | Extract Speed CSV Plotly | `extract_speed_csv_plotly/extract_speed_csv_plotly.py` | Cyclic / repeated run | Queries bag datasets by `rdpms.tsdata`, extracts a configured `Float32` speed topic to CSV, uploads the CSV to a target collection, and assigns a Plotly visualization manifest on the source dataset. |
| Template | Workflow Template | `workflow_template.sh` | Manual trigger | Bash template for wiring tool-call patterns together by logical collection roles such as `raw`, `intermediate`, and `visualization`. |

## Workflow Template

Entry point:

```bash
bash plugins/tools/develop/workflow_template.sh workflow_collection_interface_demo
```

Purpose:
- define collection ids once by role instead of repeating them in every command,
- declare independent pipelines that communicate only through collections,
- run those pipelines cyclically instead of relying on a central one-shot execution order.

Current structure:
- `COLLECTIONS[raw]`, `COLLECTIONS[intermediate]`, `COLLECTIONS[derived]`, and `COLLECTIONS[visualization]` are the main wiring points,
- `register_pipeline` declares an individual pipeline together with its description and command,
- `declare_*_pipelines` groups related pipeline declarations,
- `run_scheduler` executes the currently declared pipelines cyclically.

Intended usage:
- copy or adapt the template in place,
- replace placeholder collection ids with real ones,
- add one `register_pipeline` call per independent tool-chain,
- keep `workflow_*` functions small and declarative,
- use `RUN_FOREVER=1` when you want to demonstrate that eventual consistency emerges from repeated polling and idempotence.

Why this shape is useful:
- it makes the collection boundary explicit as the interface contract between pipelines,
- it helps demonstrate that upstream and downstream tools do not need to be launched in a precise sequence,
- it keeps orchestration logic weak on purpose: the scheduler only repeats, while the tools decide whether new work is available.

### Tool Entry Points

### 1. Upload IMU CSV

Entry point:

```bash
python plugins/tools/develop/upload_debug_imu_csv/upload_debug_imu_csv.py --collection <collection-id>
```

Purpose:
- create a synthetic IMU CSV dataset,
- upload it through the API,
- attach time-series metadata under `rdpms.tsdata`.

Expected usage:
- manual trigger,
- debug data generation,
- source input for `process_collection_csv_to_viz`.

### 2. Process Collection CSV to Viz

Entry point:

```bash
python plugins/tools/develop/process_collection_csv_to_viz/process_collection_csv_to_viz.py \
  --source-collection <source-collection-id> \
  --target-collection <target-collection-id>
```

Purpose:
- scan a source collection for CSV datasets,
- generate visualization PNGs,
- upload derived artifacts into a target collection,
- annotate source datasets with visualization metadata under `rdpms.viz`.

Execution model:
- intended to run cyclically,
- idempotence is approximated via `cache/processed_source_datasets.csv`,
- `--force` reprocesses already successful inputs.

### 3. Extract Rosbag GNSS + IMU to CSV

Entry point:

```bash
python plugins/tools/develop/extract_rosbag_gnss_imu_to_csv/extract_rosbag_gnss_imu_to_csv.py \
  --source-collection <source-collection-id> \
  --target-collection <target-collection-id>
```

Purpose:
- discover eligible ROS2 bag datasets in a source collection,
- extract `/gnss` and `/imu/data` into CSV artifacts,
- upload those artifacts as a derived dataset,
- assign or refresh `rdpms.tsdata`,
- assign `rdpms.viz` on the source dataset.

Execution model:
- intended to run cyclically,
- idempotence is approximated via `cache/processed_rosbag_source_datasets.csv`,
- `--force` reprocesses already successful inputs.

Current selection behavior:
- this script filters source datasets by existing metadata under `rdpms.tsdata`,
- specifically, it queries for datasets whose metadata already describes `/gnss` and `/imu/data` with the expected ROS2 message types.

## Annotate Script Status

Entry point:

```bash
python plugins/tools/develop/annotate_dataset_time_series_metadata/annotate_dataset_time_series_metadata.py \
  --dataset <dataset-id>
```

Current interpretation from code inspection:
- this is not part of the CSV upload workflow,
- it is not required for `process_collection_csv_to_viz`,
- it appears to be a manual bootstrap/helper script for ROS2 bag datasets,
- it can make a dataset eligible for `extract_rosbag_gnss_imu_to_csv`, because that cyclic extractor currently queries only datasets that already have matching `rdpms.tsdata`.

Limitation:
- the metadata assigned here is synthetic/manual and only uses count hints,
- `extract_rosbag_gnss_imu_to_csv` later derives richer metadata directly from the bag contents and writes `rdpms.tsdata` again.

Practical consequence:
- if the intended future workflow is “extract from raw bag without prior manual annotation”, the extractor’s discovery step should likely be revisited, because its current query assumes that a pre-existing metadata annotation already exists.

## Additional Tool Entry Points

### Annotate Rosbag TSData

Entry points:

```bash
python plugins/tools/develop/annotate_rosbag_tsdata/annotate_rosbag_tsdata.py --dataset <dataset-id>
python plugins/tools/develop/annotate_rosbag_tsdata/annotate_rosbag_tsdata.py --source-collection <collection-id>
```

Purpose:
- inspect a rosbag directly,
- summarize each topic into minimal `rdpms.tsdata`,
- assign topic `name`, `metadata.messageCount`, `metadata.firstMessageTimestamp`, `metadata.lastMessageTimestamp`, and `messageType.name`,
- optionally run collection-wide with tracker-backed idempotence.

### Register Existing S3 Rosbags

Entry point:

```bash
python plugins/tools/develop/register_existing_s3_rosbags/register_existing_s3_rosbags.py \
  --config plugins/tools/develop/register_existing_s3_rosbags/config.example.yaml \
  --secrets plugins/tools/develop/register_existing_s3_rosbags/secrets.example.yaml \
  --dry-run
```

Purpose:
- scan an existing S3 bucket/prefix for rosbag directories containing `metadata.yaml`,
- optionally restrict matched rosbag directory paths with `discovery.directory_regex` or `--regex`,
- skip datasets whose derived slug already exists in the target collection,
- register every file below each rosbag directory as an S3 reference relative to the configured datastore prefix,
- create the target dataset directly in sealed state through `POST /api/v1/data/datasets/new/sealed/s3`.

Notes:
- the main config intentionally does not contain credentials or S3 endpoint details; `rdpms.s3_store_id` is the join key into the secrets YAML,
- copy `config.example.yaml` to `config.yaml` and `secrets.example.yaml` to `secrets.yaml` for a real run,
- dataset name and slug are the rosbag directory basename,
- the API server validates every inserted S3 reference by object key and size before saving the dataset,
- the tool-specific `.gitignore` keeps local `config.yaml` and `secrets.yaml` out of the repo,
- the script requires `boto3`, `PyYAML`, and `requests` in the Python environment.

### Trim Motion Rosbag

Entry point:

```bash
python plugins/tools/develop/trim_motion_rosbag/trim_motion_rosbag.py \
  --config plugins/tools/develop/trim_motion_rosbag/config.example.yaml
```

Purpose:
- query source datasets by `rdpms.tsdata` for a configured trigger topic and ROS type,
- download new bags from a source collection,
- detect the first and last receive timestamp where motion occurs on the configured `Float32` topic,
- trim the bag to that window plus configurable padding by record timestamp,
- keep only a configured list of topics,
- copy selected messages as serialized data without deserializing every topic,
- upload the rewritten bag into a target collection,
- assign minimal `rdpms.tsdata` to the derived dataset.

Notes:
- the output bag is currently uncompressed,
- `output_storage_id` controls whether the writer creates `mcap` or `sqlite3`,
- `config.example.yaml` is only a template and should be copied or edited for a concrete workflow.

### Extract Speed CSV Plotly

Entry point:

```bash
python plugins/tools/develop/extract_speed_csv_plotly/extract_speed_csv_plotly.py \
  --source-collection <source-collection-id> \
  --target-collection <target-collection-id>
```

Purpose:
- query source datasets by `rdpms.tsdata` for a configured speed topic,
- extract that `Float32` topic into a `stamp,speed` CSV,
- upload the CSV as a derived dataset,
- assign a `rdpms.viz` manifest on the source dataset that opens the CSV with `rdpms.timeseries-plotly`.
