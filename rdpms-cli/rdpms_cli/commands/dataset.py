import sys

from tabulate import tabulate
import hashlib
import uuid
import re
import shutil

import requests

from rdpms_cli.util.TypeStore import TypeStore, get_types
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


def _load_active_instance():
    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        print('error: unknown instance key in conf!')
        exit(1)
    return conf.instances[conf.active_instance_key]


def _safe_dataset_dir_name(name: str | None, dataset_id: uuid.UUID) -> str:
    candidate = (name or '').strip()
    candidate = candidate.replace('/', '_').replace('\\', '_')
    candidate = re.sub(r'\s+', ' ', candidate).strip(' .')
    if not candidate:
        return str(dataset_id)
    return candidate


def _normalize_dataset_member_path(raw_name: str):
    from pathlib import PurePosixPath, Path

    normalized = raw_name.replace('\\', '/').strip()
    if not normalized:
        raise ValueError('dataset contains an empty file path')
    if '\x00' in normalized:
        raise ValueError(f'dataset contains an invalid file path: {raw_name!r}')
    if re.match(r'^[A-Za-z]:', normalized):
        raise ValueError(f'dataset contains an absolute Windows path: {raw_name!r}')
    if normalized.startswith('/'):
        raise ValueError(f'dataset contains an absolute path: {raw_name!r}')

    posix_path = PurePosixPath(normalized)
    if posix_path.name == '':
        raise ValueError(f'dataset contains a directory-like entry without file name: {raw_name!r}')

    clean_parts = []
    for part in posix_path.parts:
        if part in ('', '.'):
            continue
        if part == '..':
            raise ValueError(f'dataset contains a path that escapes the target directory: {raw_name!r}')
        clean_parts.append(part)

    if not clean_parts:
        raise ValueError(f'dataset contains an invalid file path: {raw_name!r}')

    return Path(*clean_parts)


def _prepare_output_root(output_arg: str | None, dataset_name: str | None, dataset_id: uuid.UUID, force: bool):
    from pathlib import Path

    root = Path(output_arg) if output_arg else Path.cwd() / _safe_dataset_dir_name(dataset_name, dataset_id)

    if root.exists():
        if not root.is_dir():
            raise ValueError(f'output path exists and is not a directory: {root}')
        if force:
            shutil.rmtree(root)
            root.mkdir(parents=True, exist_ok=True)
            return root
        if any(root.iterdir()):
            raise ValueError(f'output directory is not empty: {root} (use --force to replace it)')
        return root

    root.mkdir(parents=True, exist_ok=True)
    return root


def _validate_download_layout(root, files):
    planned_paths = {}

    for file in files:
        if file.id is None:
            raise ValueError('dataset contains a file without an id')
        if file.name is None:
            raise ValueError(f'dataset contains a file without a name (id={file.id})')

        rel_path = _normalize_dataset_member_path(file.name)
        if rel_path in planned_paths:
            raise ValueError(
                f"dataset contains multiple files mapping to the same relative path: '{file.name}' "
                f"and '{planned_paths[rel_path].name}'"
            )
        planned_paths[rel_path] = file

    relative_paths = list(planned_paths.keys())
    for rel_path in relative_paths:
        for parent in rel_path.parents:
            if parent == parent.parent:
                break
            if parent in planned_paths:
                raise ValueError(
                    f"dataset contains conflicting paths: '{planned_paths[parent].name}' would block '{rel_path}'"
                )

    return [(planned_paths[path], root / path) for path in sorted(relative_paths)]


def cmd_dataset_upload(args):
    from pathlib import Path
    import datetime
    from rdpms_cli.openapi_client import ApiClient, Configuration
    from rdpms_cli.openapi_client import DataSetsApi, DataSetCreateRequestDTO, S3FileCreateRequestDTO
    from rdpms_cli.openapi_client.exceptions import BadRequestException, ApiException

    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        print(f'error: unknown instance key in conf!')
        exit(1)

    pth = Path(args.path)
    if not pth.exists():
        print(f'error: path does not exist!')
        exit(1)

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    client = ApiClient(api_conf)
    ds_api = DataSetsApi(client)

    stamp = datetime.datetime.now(datetime.UTC)
    name = args.name
    if not name:
        name = pth.name

    print(f"[dataset upload] Uploading dataset from: {args.path}, name: {name}, collection: {args.collection}")

    ds_req_dto = DataSetCreateRequestDTO(
        name=name,
        slug=name,
        createdStampUTC=stamp,
        collectionId=args.collection
    )
    try:
        dataset = ds_api.api_v1_data_datasets_new_post(ds_req_dto)
    except ApiException as api_Ex:
        print(f'encountered HTTP 400', file=sys.stderr)
        print(f'Reason: {api_Ex.reason}')
        print(f'Body:\n{api_Ex.body}\n')
        exit(1)

    types = get_types('', client)

    def upload_file(ds_id: uuid.UUID, base_path: Path, file_path: Path, type_store: TypeStore):
        stats = (base_path / file_path).stat()
        sha256 = hashlib.new('sha256')
        with open(base_path / file_path, 'rb') as f:
            while True:
                data = f.read(65536)
                if not data:
                    break
                sha256.update(data)

        create_stamp = datetime.datetime.fromtimestamp(stats.st_ctime, datetime.UTC)
        content_type = type_store.resolve_by_ending(file_path.name)
        # print(f'\tstats for {file_path}: size={stats.st_size}, sha256={sha256.hexdigest()}, content_type={content_type.display_name}, created={create_stamp}')
        upload_req = S3FileCreateRequestDTO(
            name=str(file_path),
            size_bytes=stats.st_size,
            plain_sha256_hash=sha256.hexdigest(),
            created_stamp=create_stamp,
            content_type_id=content_type.id
        )
        upload_resp = ds_api.api_v1_data_datasets_id_add_s3_post(ds_id, upload_req)

        if stats.st_size == 0:
            # Explicitly tell the server "body is empty and length is 0"
            resp = requests.put(upload_resp.upload_uri, data=b'', headers={
                'Content-Length': '0',
            })
            print(f'\tupload complete ({resp.status_code}) for {file_path}')
        else:
            with open(base_path / file_path, 'rb') as f:
                resp = requests.put(upload_resp.upload_uri, data=f)
            print(f'\tupload complete ({resp.status_code}) for {file_path}')

    if pth.is_dir():
        for dirpath, dirnames, filenames in pth.walk():
            for child in filenames:
                relative_file = (dirpath / child).relative_to(pth)
                print(f'uploading file: {child} (in {dirpath}) -> {relative_file}')
                upload_file(dataset.id, pth, relative_file, types)
    else:
        upload_file(dataset.id, Path('.'), pth, types)

    # seal dataset
    try:
        ds_api.api_v1_data_datasets_id_seal_put(dataset.id)
    except ApiException as api_ex:
        print(f'encountered HTTP {api_ex.status}', file=sys.stderr)
        print(f'Reason: {api_ex.reason}')
        print(f'Body:\n{api_ex.body}\n')

def cmd_dataset_download(args):
    from pathlib import Path
    from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
    from rdpms_cli.openapi_client.exceptions import ApiException

    try:
        dataset_id = uuid.UUID(args.dataset_id)
    except ValueError:
        print(f'error: invalid dataset id: {args.dataset_id}', file=sys.stderr)
        exit(1)

    instance = _load_active_instance()
    client = _build_api_client()
    ds_api = DataSetsApi(client)

    try:
        dataset = ds_api.api_v1_data_datasets_id_get(dataset_id)
        output_root = _prepare_output_root(args.output, dataset.name, dataset_id, args.force)
        planned_downloads = _validate_download_layout(output_root, dataset.files or [])
    except ApiException as api_ex:
        print(f'encountered HTTP {api_ex.status}', file=sys.stderr)
        print(f'Reason: {api_ex.reason}', file=sys.stderr)
        print(f'Body:\n{api_ex.body}\n', file=sys.stderr)
        exit(1)
    except ValueError as ex:
        print(f'error: {ex}', file=sys.stderr)
        exit(1)

    print(f"[dataset download] Downloading dataset {dataset_id} to: {output_root}")

    if not planned_downloads:
        print('dataset contains no files')
        return

    headers = {}
    if instance.token:
        headers['Authorization'] = f'Bearer {instance.token}'

    for file, target_path in planned_downloads:
        if not file.download_uri:
            print(f'error: file has no download URI: {file.name}', file=sys.stderr)
            exit(1)

        target_path.parent.mkdir(parents=True, exist_ok=True)
        if target_path.exists() and not target_path.is_file():
            print(f'error: target path is not a file: {target_path}', file=sys.stderr)
            exit(1)

        response = None
        bytes_written = 0
        try:
            response = requests.get(file.download_uri, headers=headers, stream=True, allow_redirects=True)
            if response.status_code >= 400:
                body = response.text[:1000]
                raise RuntimeError(
                    f'HTTP {response.status_code} while downloading {file.download_uri}: {body}'
                )
            with open(target_path, 'wb') as target_file:
                for chunk in response.iter_content(chunk_size=65536):
                    if not chunk:
                        continue
                    target_file.write(chunk)
                    bytes_written += len(chunk)
        except Exception as ex:
            if target_path.exists():
                target_path.unlink()
            print(f'error: failed to download {file.name} -> {Path(target_path).relative_to(output_root)}: {ex}', file=sys.stderr)
            exit(1)
        finally:
            if response is not None:
                response.close()

        if file.size is not None and bytes_written != file.size:
            target_path.unlink(missing_ok=True)
            print(
                f'error: size mismatch for {file.name}: expected {file.size} bytes, got {bytes_written}',
                file=sys.stderr
            )
            exit(1)

        print(f'\tdownloaded {bytes_written} bytes -> {Path(target_path).relative_to(output_root)}')


def cmd_dataset_new(args):
    print(f"[dataset new] Creating dataset from: {args.path}, name: {args.name}, collection: {args.collection}")


def cmd_dataset_seal(args):
    print(f"[dataset seal] Sealing dataset: {args.dataset_id}")


def cmd_dataset_metadata(args):
    print(f"[dataset metadata] Showing metadata for dataset: {args.dataset_id}")


def cmd_dataset_list(args):
    from rdpms_cli.openapi_client import ApiClient, Configuration
    from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi

    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        print(f'error: unknown instance key in conf!')
        exit(1)

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    client = ApiClient(api_conf)
    ds_api = DataSetsApi(client)

    cid = None
    if args.collection:
        cid = uuid.UUID(args.collection)

    ds_list = ds_api.api_v1_data_datasets_get(collection_id=cid)

    if not ds_list:
        print('no datasets found')
        return
    
    datasets = ds_list

    if args.collection:
        header = ["dataset name", "file count", "id"]
        table = [[ds.name, ds.file_count, ds.id] for ds in datasets]
    else:
        header = ["dataset name", "file count", "id", "collection id"]
        table = [[ds.name, ds.file_count, ds.id, ds.collection_id] for ds in datasets]

    print(tabulate(headers=header, tabular_data=table))

def cmd_dataset_describe(args):
    print(f"[dataset describe] Describing dataset: {args.dataset_id}")
