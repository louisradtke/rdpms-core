from tabulate import tabulate

from rdpms_cli.util.config_store import load_file


def cmd_collection_list(args):
    from rdpms_cli.openapi_client import ApiClient, Configuration
    from rdpms_cli.openapi_client.api.collections_api import CollectionsApi

    conf = load_file()
    if conf.active_instance_key not in conf.instances:
        print(f'error: unknown instance key in conf!')
        exit(1)

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    client = ApiClient(api_conf)
    c_api = CollectionsApi(client)

    pid = None
    if args.project:
        pid = args.project

    c_list = c_api.api_v1_data_collections_get(project_id=pid)

    if not c_list:
        print('no collections found')
        return

    if args.project:
        header = ["dataset name", "dataset count", "id"]
        table = [[c.name, c.data_set_count, c.id] for c in c_list]
    else:
        header = ["dataset name", "dataset count", "id", "project id"]
        table = [[c.name, c.data_set_count, c.id, c.project_id] for c in c_list]

    print(tabulate(headers=header, tabular_data=table))


def cmd_collection_new(args):
    print(f"[collection create] Creating collection '{args.name}' with description: {args.description}")
