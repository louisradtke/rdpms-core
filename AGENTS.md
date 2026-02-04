# AGENTS.md for RDPMS Core

## Scope
This repository hosts the core system for RDPMS (Research Data and Pipeline Management System). It includes:
- A .NET backend API and services
- A Python CLI
- A Svelte-based web UI
- Supporting configs, docs, and plugins

## Domain Context (current understanding)
RDPMS centers on a data store and catalog for research/robotics datasets. The backend manages data files, data sets, collections, metadata, and pipelines. Storage can be backed by PostgreSQL and S3-compatible object storage (MinIO in dev).

### Data Model (nutshell)
- Collection -> Dataset -> Files -> Storage references
- Data Store -> Storage references for physical access

## Repository Layout
- `rdpms-backend/` .NET 9 backend (ASP.NET Core + EF Core)
- `rdpms-cli/` Python CLI client
- `rdpms-web-ui/` Svelte 5 + Vite 6 web UI
- `plugins/` plugin-related code
- `config/`, `docs/`, `scripts/` misc support
- `swagger.yaml` API spec snapshot

## Backend Notes (.NET)
- Solution: `rdpms-backend/rdpms-backend.sln`
- Server project: `rdpms-backend/src/RDPMS.Core.Server`
- Target framework: .NET 9 (`net9.0`)
- API versioning and Swagger UI enabled
- Default config file: `rdpms-backend/src/RDPMS.Core.Server/debug.yaml` for local dev
- Example dev config (Postgres for compose dev stack (`full` profile)): `rdpms-backend/develop/confs/dev-postgres.yaml`
- Note: Postgres DB init/seeding is currently untested

### Common backend commands (verify if preferred)
- Run server locally:
  - Seed DB: `dotnet run --project rdpms-backend/src/RDPMS.Core.Server -- seed --init-db=dev`
  - Serve API: `dotnet run --project rdpms-backend/src/RDPMS.Core.Server -- serve`
- Run seed-only:
  - `dotnet run --project rdpms-backend/src/RDPMS.Core.Server -- seed --config <path-to-yaml> --init-db=dev`
- Docker dev stack (deps or full):
  - `docker compose -f rdpms-backend/compose.yaml --profile deps up`
  - `docker compose -f rdpms-backend/compose.yaml --profile full up`
  - `full` is WIP and still needs the UI started separately

### Tests
- NUnit project: `rdpms-backend/test/RDPMS.Core.Tests.Data`
- Example: `dotnet test rdpms-backend/rdpms-backend.sln`

## CLI Notes (Python)
- Package: `rdpms-cli`
- Entry point: `rdpms` (from `rdpms-cli/rdpms_cli/main_cli.py`)
- Python: `>=3.10`
- Install (dev): `pip install --editable rdpms-cli` (ideally inside a venv)
  - Preferred: create venv in `rdpms-cli/.venv`

## Web UI Notes (Svelte)
- Location: `rdpms-web-ui/`
- Svelte 5 + Vite 6 + Tailwind 4
- Scripts in `rdpms-web-ui/package.json` (`dev`, `build`, `check`, `lint`)
- Package manager preference: `npm`

## Conventions
- All timestamps are UTC.
- Avoid the ambiguous term “Collection” in .NET entity naming; prefer `...CollectionEntity` when ambiguity might arise.

## Secrets and Dev Defaults
- S3 dev keys are currently stored in the DB and seeded via `rdpms-backend/src/RDPMS.Core.Persistence/DefaultValues.cs`.

## Dev Environment Notes
- CLI is installed/run on the host (not in the compose stack).
- JetBrains Rider setup:
  - Rider project is in `rdpms-backend/`.
  - "Full Dev-Stack" profile runs dependencies via compose `deps` and runs backend + web UI.
  - Dev deps: .NET, Node + npm, Docker or Podman (plus a Podman VM if using Podman).
  - If using Podman VM, set the Engine API URL (unix socket path) in the IDE/plugin settings.

## Migrations
- Add a new EF migration:
  - `dotnet ef migrations add -p rdpms-backend/src/RDPMS.Core.Persistence/RDPMS.Core.Persistence.csproj <INTEGRATION_NAME>`

## Agent Behavior
- If changes touch project conventions, workflows, or architecture, suggest updating this `AGENTS.md` and offer to make the edit.

## Backend Structure (ASP.NET Core)
### Project/Layer Overview
- `RDPMS.Core.Contracts`: Shared contract types (e.g., file/container metadata records). We will create JSON schemas from here and create types in other languages, esp. TS, from them. These contracts model file/container metadata to enable reasoning across components.
- `RDPMS.Core.Infra`: Cross-cutting infra (config, CLI options, constants, helpers, exceptions).
- `RDPMS.Core.Persistence`: EF Core entities, DbContext, migrations, default seed values, slug helpers.
- `RDPMS.Core.Server`: Web API (controllers), services, repositories, DTOs/mappers, utilities.
- `RDPMS.MockJsonValidation`: Experimental UI for JSON/schema validation (not part of runtime API).
- `RDPMS.MockS3Interaction`: Minimal server for S3 interaction tests (dev helper).

### Server-Layer Interaction
- Controllers (HTTP) -> Services (domain/business logic) -> Persistence (EF Core DbContext + entities).
- Services primarily use `GenericCollectionService<T>` / `ReadonlyGenericCollectionService<T>` for CRUD/query patterns.
- DTO Mappers translate between Persistence models and API DTOs.
- Repositories in `Model/Repositories` provide specialized queries used by services (not direct controller usage).

### Services (RDPMS.Core.Server/Services)
- `ContentTypeService`: CRUD for content types; includes lookup by MIME type with project scope.
- `DataCollectionEntityService`: CRUD for collections, includes dataset counts and metadata column upsert.
- `DataFileService`: File storage behavior (S3 presigned URLs, DB storage, download URI resolution).
- `DataSetService`: Dataset CRUD, slug validation, sealing workflow, metadata validation status.
- `MetadataService`: Metadata JSON storage in DB, schema validation, assign/replace metadata links.
- `ProjectService`: Project CRUD, global project retrieval, update name/slug.
- `SchemaService`: CRUD for JSON schemas.
- `S3Service`: MinIO/S3 client management, presigned URLs, and file validation.
- `SecretResolverService`: Resolve secret URLs (currently supports `direct://`).
- `SlugService`: Slug registration, uniqueness, and resolution across entities.
- `SlugResolvingService<T>`: Generic lookup by slug for entities with slugs.
- `GenericCollectionService<T>` / `ReadonlyGenericCollectionService<T>`: Base services for common CRUD/query behavior.

### Controllers (RDPMS.Core.Server/Controllers/V1)
- `CollectionsController`: List/create collections, fetch by id, upsert metadata columns on collections.
- `ContentTypesController`: List/create content types, batch add, get by id.
- `DataSetsController`: List/create datasets, get by id, delete (mark pending), seal, add S3 file, assign metadata.
- `FilesController`: List/get files, download content redirect, download blob, list storage references.
- `MetaDataController`: List/get metadata, validate metadata against schema, list/add schemas, fetch schema blob.
- `ProjectsController`: List/get projects, update project name/slug, collection dataset counts and slugs.
- `StoresController`: List/get data stores, filter by storage type.
