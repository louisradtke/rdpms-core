import json
import uuid
from pathlib import Path

from rdpms_cli.util.config_store import load_file


def _build_api_client():
    from rdpms_cli.openapi_client import ApiClient, Configuration

    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        print('error: unknown instance key in conf!')
        exit(1)

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    return ApiClient(api_conf)


def _read_json_payload(args) -> str:
    if getattr(args, 'json', None) and getattr(args, 'json_file', None):
        raise ValueError('only one of --json or --json-file can be provided')

    if getattr(args, 'json', None):
        payload = args.json
    elif getattr(args, 'json_file', None):
        payload = Path(args.json_file).read_text(encoding='utf-8')
    else:
        raise ValueError('metadata assignment to files and datasets requires --json or --json-file')

    # Validate JSON early and send compact canonical form.
    return json.dumps(json.loads(payload), separators=(',', ':'))


def _parse_metadata_target(target: str):
    from rdpms_cli.openapi_client.models.metadata_column_target_dto import MetadataColumnTargetDTO

    normalized = target.strip().lower()
    if normalized == 'dataset':
        return MetadataColumnTargetDTO.DATASET
    if normalized == 'file':
        return MetadataColumnTargetDTO.FILE
    raise ValueError("invalid --target, expected 'dataset' or 'file'")


def cmd_metadata_show(args):
    print(f'[metadata show] Showing metadata for resource: {args.resource_id}, type: {args.type}')


def cmd_metadata_assign(args):
    from rdpms_cli.openapi_client.exceptions import ApiException

    try:
        resource_type = args.resource_type
        resource_id = uuid.UUID(args.resource_id)
        key = args.key
        client = _build_api_client()

        if resource_type == 'dataset':
            from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
            payload = _read_json_payload(args)
            out = DataSetsApi(client).api_v1_data_datasets_id_metadata_key_put(
                id=resource_id,
                key=key,
                body=payload
            )
            print(f'assigned dataset metadata key={key} meta_id={out.id}')
            return

        if resource_type == 'file':
            from rdpms_cli.openapi_client.api.files_api import FilesApi
            payload = _read_json_payload(args)
            out = FilesApi(client).api_v1_data_files_id_metadata_key_put(
                id=resource_id,
                key=key,
                body=payload
            )
            print(f'assigned file metadata key={key} meta_id={out.id}')
            return

        if resource_type == 'collection':
            from rdpms_cli.openapi_client.api.collections_api import CollectionsApi

            if args.json or args.json_file:
                raise ValueError('collection metadata column assignment does not accept --json/--json-file')
            if not args.target:
                raise ValueError("collection metadata column assignment requires --target ('dataset' or 'file')")

            schema_id = uuid.UUID(args.schema_id) if args.schema_id else None
            default_metadata_id = uuid.UUID(args.default_metadata_id) if args.default_metadata_id else None
            target = _parse_metadata_target(args.target)

            CollectionsApi(client).api_v1_data_collections_id_metadata_key_put(
                id=resource_id,
                key=key,
                schema_id=schema_id,
                default_metadata_id=default_metadata_id,
                target=target
            )
            print(
                f'assigned collection metadata column key={key} target={target.value} '
                f'schema_id={schema_id} default_metadata_id={default_metadata_id}'
            )
            return

        raise ValueError(f'unsupported resource_type: {resource_type}')
    except (ValueError, json.JSONDecodeError) as err:
        print(f'error: {err}')
        exit(2)
    except ApiException as err:
        print(f'api error: {err}')
        exit(1)


def cmd_metadata_set(args):
    cmd_metadata_assign(args)


def cmd_metadata_validate(args):
    print(f'[metadata validate] Validating metadata for resource: {args.resource_id}, schema: {args.schema}')
