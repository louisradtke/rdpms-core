# Contracts and Schemas

From a technical perspective, there are two pre-defined interfaces/contracts with different sources of truth:
1. The REST API, defined by the API-controllers inside the rdpms-backend .NET solution (`RDPMS.Core.Server.Controllers`).
2. Schemas for the metadata, defined in the `schemas/json` directory.

The RDPMS is designed to communicate and reason over metadata (in JSON format). Thus, metadata needs a well-defined structure (syntax, defined by a schema) to make the data accessible for logic. The integration of vocabularies and ontologies is a future goal, creating a well-defined semantic model.

## Sources of Truth and Type Generation

THIS SECTION IS WIP

Code gets generated from the schemas. Targets are .NET and TypeScript. The generated code is located in two places:
- `rdpms-backend/src/RDPMS.Core.Contracts/Schemas`
- `rdpms-web-ui/src/lib/contracts/schemas`

The schemas dir also features a `schema_uuids.json`, which contains a mapping between schemas and constant UUIDs. This is (or might be) important for reproducible seeding of the backend database. The backend can access this information (mapping) via the `RDPMS.Core.Contracts.EmbeddedSchemaRepository` class.


The .NET generation is based on the tools around [`Corvus.JsonSchema`](https://github.com/corvus-dotnet/Corvus.JsonSchema). Run the .NET generation (from repo root) via:

```bash
docker run -it -v $PWD/schemas:/work/schemas -v $PWD/rdpms-backend/src/RDPMS.Core.Contracts/Schemas:/work/out rdpms/dotnet-type-generator
```

The TypeScript generation is based on [`json-schema-to-typescript`](https://github.com/bcherny/json-schema-to-typescript). Run it via (from repo root):

```bash
docker run -it -v $PWD/schemas:/work/schemas -v $PWD/rdpms-web-ui/src/lib/contracts/schemas:/work/out rdpms/ts-type-generator:latest
```
