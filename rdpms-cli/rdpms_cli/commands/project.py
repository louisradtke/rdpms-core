def cmd_project_list(args):
    print("[project list] Listing all projects")


def cmd_project_select(args):
    print(f"[project select] Selecting project: {args.project_id}")


def cmd_project_create(args):
    print(f"[project create] Creating project '{args.name}' with description: {args.description}")
