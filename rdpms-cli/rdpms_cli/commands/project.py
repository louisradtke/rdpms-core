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
    
    dto = project_api.api_v1_projects_get()
    
    for project in dto:
        print(f'{project.name} (collections: {len(project.collections)}, stores: {len(project.data_stores)})')


def cmd_project_select(args):
    print(f"[project select] Selecting project: {args.project_id}")


def cmd_project_create(args):
    print(f"[project create] Creating project '{args.name}' with description: {args.description}")
