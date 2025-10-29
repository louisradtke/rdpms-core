from tabulate import tabulate

from rdpms_cli.util.config_store import load_file


def cmd_project_list(args):
    from rdpms_cli.openapi_client import ApiClient, Configuration
    from rdpms_cli.openapi_client.api.projects_api import ProjectsApi

    conf = load_file()

    if conf.active_instance_key not in conf.instances:
        print(f'error: unknown instance key in conf!')
        exit(1)

    instance = conf.instances[conf.active_instance_key]
    api_conf = Configuration(host=instance.base_url)
    client = ApiClient(api_conf)
    project_api = ProjectsApi(client)
    
    project_list = project_api.api_v1_projects_get()

    if not project_list:
        print('no projects found')
        return
    
    header = ["project name", "# collections", "# stores"]
    table = [[p.name, len(p.collections), len(p.data_stores)] for p in project_list]    
    print(tabulate(headers=header, tabular_data=table))


def cmd_project_select(args):
    print(f"[project select] Selecting project: {args.project_id}")


def cmd_project_create(args):
    print(f"[project create] Creating project '{args.name}' with description: {args.description}")
