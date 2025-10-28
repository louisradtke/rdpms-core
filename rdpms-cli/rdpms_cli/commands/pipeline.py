def cmd_pipeline_run(args):
    print(f"[pipeline run] Running pipeline: {args.pipeline_id}")


def cmd_pipeline_list(args):
    print("[pipeline list] Listing all pipelines")


def cmd_pipeline_status(args):
    print(f"[pipeline status] Checking status of job: {args.job_id}")
