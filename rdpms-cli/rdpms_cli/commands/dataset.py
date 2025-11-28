import sys
from pstats import Stats

from tabulate import tabulate
import hashlib

import requests

from rdpms_cli.util.TypeStore import TypeStore, get_types
from rdpms_cli.util.config_store import load_file


def cmd_dataset_upload(args):
    from pathlib import Path
    import datetime
    from rdpms_cli.openapi_client import ApiClient, Configuration
    from rdpms_cli.openapi_client import DataSetsApi, DataSetSummaryDTO, S3FileCreateRequestDTO
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

    ds_req_dto = DataSetSummaryDTO(
        name=name,
        slug=name,
        createdStampUTC=stamp,
        collectionId=args.collection
    )
    try:
        ds_id = ds_api.api_v1_data_datasets_post(ds_req_dto)
    except ApiException as api_Ex:
        print(f'encountered HTTP 400', file=sys.stderr)
        print(f'Reason: {api_Ex.reason}')
        print(f'Body:\n{api_Ex.body}\n')
        exit(1)

    types = get_types('', client)

    def upload_file(ds_id: str, base_path: Path, file_path: Path, type_store: TypeStore):
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
            sizeBytes=stats.st_size,
            plainSHA256Hash=sha256.hexdigest(),
            createdStamp=create_stamp,
            contentTypeId=content_type.id
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
                upload_file(ds_id, pth, relative_file, types)
    else:
        upload_file(ds_id, Path('.'), pth, types)

    # seal dataset
    try:
        ds_api.api_v1_data_datasets_id_seal_put(ds_id)
    except ApiException as api_ex:
        print(f'encountered HTTP {api_ex.status}', file=sys.stderr)
        print(f'Reason: {api_ex.reason}')
        print(f'Body:\n{api_ex.body}\n')

def cmd_dataset_download(args):
    print(f"[dataset download] Downloading dataset {args.dataset_id} to: {args.output}")


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
        cid = args.collection

    ds_list = ds_api.api_v1_data_datasets_get()
    
    if not ds_list:
        print('no datasets found')
        return
    
    if args.collection:
        header = ["dataset name", "file count", "id"]
        table = [[ds.name, ds.file_count, ds.id] for ds in ds_list]
    else:
        header = ["dataset name", "file count", "id", "collection id"]
        table = [[ds.name, ds.file_count, ds.id, ds.collection_id] for ds in ds_list]

    print(tabulate(headers=header, tabular_data=table))

def cmd_dataset_describe(args):
    print(f"[dataset describe] Describing dataset: {args.dataset_id}")
