from rdpms_cli.openapi_client import ApiClient, Configuration
from rdpms_cli.openapi_client.api.data_sets_api import DataSetsApi
from rdpms_cli.util.TypeStore import TypeStore

print(f"[dataset upload] Uploading dataset from: {args.path}, name: {args.name}, collection: {args.collection}")

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

def get_types(client: ApiClient) -> TypeStore:
    pass