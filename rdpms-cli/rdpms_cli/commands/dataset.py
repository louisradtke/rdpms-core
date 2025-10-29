from tabulate import tabulate

from rdpms_cli.util.config_store import load_file


def cmd_dataset_upload(args):
    print(f"[dataset upload] Uploading dataset from: {args.path}, name: {args.name}, collection: {args.collection}")


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
    from rdpms_cli.openapi_client.api.collections_api import CollectionsApi

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
