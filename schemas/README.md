# Schemas

This directory contains JSON Schemas that describe the shared contracts used across the system.

## Layout
- `schemas/json/` contains JSON Schema files. This is the source of truth for this type of contracts.

## Conventions
- Filenames are versioned (e.g. `message-type.v1.schema.json`).
- Versioning is not stable yet and will change while the schema design is iterated.
- Each schema includes `$id`, `$schema`, and a version marker.
- Schemas are language-agnostic and used to generate .NET and TypeScript types.

## Schema Overview
### File + Content Model
- `file-information` is the top-level description of a file. It references `file-metadata` (blob metadata) and `file-content` (content semantics).
- `file-content` is a `oneOf` between `compressed-content` and `container-content`.
- `compressed-content` references another `file-information` to model compression chains (e.g. tar.gz).
- A file is either a compressed representation of another file or a container for one or more kinds of contained information.
- `container-content` can describe multiple facets of contained information:
  - `filesContainer` -> `file-container` -> `contained-file-information` -> `file-information`
  - `timeSeriesContainer` -> `time-series-container` -> `topic` -> `message-type`
  - `otherInformation` for structured content like CSV headers or JSON schemas (currently free-form).

### Message Model
- `message-type` holds an array of `field-definition` entries.
- Each `field-definition` names a field and its `type`, which is either a `field-type` (primitive) or another `message-type` (recursive).
- `topic-metadata` stores optional aggregates about topic data (timestamps, message counts).

### Visualization Model
- `visualization-manifest` defines dataset visualizations made up of views and items.
- The visualization manifest does not include a dataset reference; RDPMS manages the linkage externally.
- Each `visualization-view` holds `visualization-item` entries that reference a file via `file-source` and optional renderer config via `visualization-renderer`.
- Visualization items reference files by UUID (`fileId`).
- `visualization-renderer.kind` is a list of renderer IDs; `visualization-renderer.default` should be one of them (not enforceable in plain JSON Schema).
- Plugin/renderer IDs are namespaced; `rdpms.*` is reserved for built-ins.

## Technical Design Notes
- Optional fields are represented as absent (no `null` unless explicitly added in the future).

## Generation
- The generation pipeline will be added later.
