# Schemas

This directory contains JSON Schemas that describe the shared contracts used across the system.

## Layout
- `schemas/json/` contains JSON Schema files. This is the source of truth for this type of contracts.

## Conventions
- Filenames are versioned (e.g. `message-type.v1.schema.json`).
- Versioning is not stable yet and will change while the schema design is iterated.
- Each schema includes `$id`, `$schema`, and a version marker.
- Schemas are language-agnostic and used to generate .NET and TypeScript types.

## How The Schemas Fit Together
- `file-information` is the top-level description of a file. It references `file-metadata` (blob metadata) and `file-content` (content semantics).
- `file-content` is a `oneOf` between `compressed-content` and `container-content`.
- `compressed-content` references another `file-information` to model compression chains (e.g. tar.gz).
- `container-content` can describe multiple facets of contained information:
  - `filesContainer` -> `file-container` -> `contained-file-information` -> `file-information`
  - `timeSeriesContainer` -> `time-series-container` -> `topic` -> `message-type`
  - `otherInformation` for structured content like CSV headers or JSON schemas (currently free-form).
- `message-type` holds an array of `field-definition` entries.
- Each `field-definition` names a field and its `type`, which is either a `field-type` (primitive) or another `message-type` (recursive).
- `topic-metadata` stores optional aggregates about topic data (timestamps, message counts).

## Generation
- The generation pipeline will be added later.
