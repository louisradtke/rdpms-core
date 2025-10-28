def cmd_collection_list(args):
    print("[collection list] Listing all collections")


def cmd_collection_create(args):
    print(f"[collection create] Creating collection '{args.name}' with description: {args.description}")


def cmd_collection_describe(args):
    print(f"[collection describe] Describing collection: {args.collection_id}")
