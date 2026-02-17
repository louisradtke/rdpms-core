# RDPMS CLI Usage

This page describes the currently available RDPMS CLI commands and their state.
It distinguishes between commands that are implemented and commands that are currently placeholders.

## Prerequisites

- Install the CLI in your Python environment.
- Ensure an RDPMS API server is reachable.
- Configure at least one instance before using API-backed commands.

## Command Overview

### Instance commands

- `rdpms instance add <name> <url> [--token <token>] [-d|--set-default]`
  - Status: implemented
  - Stores an API endpoint in local config.
- `rdpms instance list` / `rdpms instance ls`
  - Status: implemented
  - Lists configured instances and current default.
- `rdpms instance select <name>`
  - Status: placeholder (prints a message only)
- `rdpms instance remove <name>`
  - Status: placeholder (prints a message only)

### Project commands

- `rdpms project list` / `rdpms p ls`
  - Status: implemented
  - Fetches and prints projects.
- `rdpms project select <project_id>`
  - Status: placeholder
- `rdpms project create <name> [--description <text>]`
  - Status: placeholder

### Collection commands

- `rdpms collection list` / `rdpms c ls [--project <project_id>]`
  - Status: implemented
  - Lists collections (optionally filtered by project).
- `rdpms collection new <name> [--description <text>]`
  - Status: placeholder

### Dataset commands

- `rdpms dataset upload <path> [--name <name>] [--collection <collection_id>]`
  - Status: implemented
  - Creates dataset, uploads files, then seals dataset.
- `rdpms dataset list` / `rdpms ds ls [--collection <collection_id>]`
  - Status: implemented
  - Lists datasets.
- `rdpms dataset download <dataset_id> [--output <path>]`
  - Status: placeholder
- `rdpms dataset new [--name <name>] [--collection <collection_id>]`
  - Status: currently wired to upload handler; treat as not implemented for clean creation flow
- `rdpms dataset seal <dataset_id>`
  - Status: placeholder
- `rdpms dataset metadata <dataset_id>`
  - Status: placeholder
- `rdpms dataset describe <dataset_id>`
  - Status: placeholder

### Metadata commands

- `rdpms metadata assign ...` (alias: `rdpms metadata set ...`)
  - Status: implemented
  - Supports assignment for `dataset`, `file`, and `collection` targets.
- `rdpms metadata show <resource_id> [--type ...]`
  - Status: placeholder
- `rdpms metadata validate <resource_id> [--schema <schema_file>]`
  - Status: placeholder

### Pipeline commands

- `rdpms pipeline run <pipeline_id>`
- `rdpms pipeline list` / `rdpms pipeline ls`
- `rdpms pipeline status <job_id>`
  - Status: all placeholders

### Session commands

- `rdpms login`
- `rdpms logout`
- `rdpms whoami`
  - Status: placeholders

## Metadata Assignment Usage

### 1. Assign metadata to a dataset

```bash
rdpms metadata assign dataset <dataset_id> <key> --json '{"sensor":"camera","rate_hz":30}'
```

From file:

```bash
rdpms metadata assign dataset <dataset_id> <key> --json-file ./metadata.json
```

### 2. Assign metadata to a file

```bash
rdpms metadata assign file <file_id> <key> --json '{"encoding":"rgb8"}'
```

### 3. Configure a collection metadata column

For collection columns, `--target` is required. Supported targets are `dataset` and `file`.

```bash
rdpms metadata assign collection <collection_id> <key> --target dataset --schema-id <schema_uuid>
```

Optional default metadata reference:

```bash
rdpms metadata assign collection <collection_id> <key> --target file --default-metadata-id <metadata_uuid>
```

Notes:

- Collection assignment does not accept `--json`/`--json-file`.
- Dataset/file assignment requires either `--json` or `--json-file`.
- `metadata set` is an alias of `metadata assign`.

## Recommended First Steps

1. Register your API instance:

```bash
rdpms instance add dev http://localhost:5000 --set-default
```

2. Verify connectivity with implemented list commands:

```bash
rdpms p ls
rdpms c ls
rdpms ds ls
```

3. Use metadata assignment as needed:

```bash
rdpms metadata assign dataset <dataset_id> <key> --json-file ./meta.json
```
