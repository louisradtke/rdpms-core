
def cmd_dataset_upload(args):
    print(f"[dataset upload] Uploading dataset from: {args.path}, name: {args.name}, collection: {args.collection}")


def cmd_dataset_download(args):
    print(f"[dataset download] Downloading dataset {args.dataset_id} to: {args.output}")


def cmd_dataset_seal(args):
    print(f"[dataset seal] Sealing dataset: {args.dataset_id}")


def cmd_dataset_metadata(args):
    print(f"[dataset metadata] Showing metadata for dataset: {args.dataset_id}")


def cmd_dataset_list(args):
    print("[dataset list] Listing all datasets")


def cmd_dataset_describe(args):
    print(f"[dataset describe] Describing dataset: {args.dataset_id}")
