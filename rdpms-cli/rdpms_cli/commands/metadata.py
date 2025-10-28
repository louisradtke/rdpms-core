def cmd_metadata_show(args):
    print(f"[metadata show] Showing metadata for resource: {args.resource_id}, type: {args.type}")


def cmd_metadata_set(args):
    print(f"[metadata set] Setting {args.key}={args.value} for resource: {args.resource_id}, type: {args.type}")


def cmd_metadata_validate(args):
    print(f"[metadata validate] Validating metadata for resource: {args.resource_id}, schema: {args.schema}")
