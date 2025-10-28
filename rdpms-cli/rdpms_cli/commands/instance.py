from rdpms_cli.util.config_store import store_file, load_file, RdpmsInstance

def cmd_instance_add(args):
    conf = load_file()
    
    instance = RdpmsInstance(base_url=args.url, token=args.token)

    conf.instances[args.name] = instance
    if args.set_default:
        conf.active_instance_key = args.name
    
    store_file(conf)

    print(f'stored system "{args.name}" with URL {instance.base_url}')
    if args.set_default:
        print(f'new default set')

def cmd_instance_list(args):
    print("[instance list] Listing all instances")


def cmd_instance_select(args):
    print(f"[instance select] Selecting instance: {args.name}")


def cmd_instance_remove(args):
    print(f"[instance remove] Removing instance: {args.name}")
