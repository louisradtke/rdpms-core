#!/usr/bin/env python3

from argparse import ArgumentParser
from typing import Union

from rdpms_cli.commands.collection import cmd_collection_new, cmd_collection_list
from rdpms_cli.commands.dataset import (cmd_dataset_upload, cmd_dataset_download, cmd_dataset_seal,
                                        cmd_dataset_metadata, cmd_dataset_list, cmd_dataset_describe)
from rdpms_cli.commands.metadata import cmd_metadata_show, cmd_metadata_set, cmd_metadata_validate
from rdpms_cli.commands.project import cmd_project_list, cmd_project_select, cmd_project_create
from rdpms_cli.commands.instance import cmd_instance_add, cmd_instance_list, cmd_instance_select, cmd_instance_remove
from rdpms_cli.commands.pipeline import cmd_pipeline_run, cmd_pipeline_list, cmd_pipeline_status


def build_parser() -> ArgumentParser:
    parser = ArgumentParser(
        prog='rdpms',
        description='Research Data Project Management System CLI'
    )

    subparsers = parser.add_subparsers(dest='command', help='Available commands')

    # login command
    login_parser = subparsers.add_parser('login', help='Authenticate and store token')
    login_parser.add_argument('--username', help='Username for authentication')
    login_parser.add_argument('--password', help='Password for authentication')
    login_parser.set_defaults(func=cmd_login)

    # logout command
    logout_parser = subparsers.add_parser('logout', help='Logout and remove stored token')
    logout_parser.set_defaults(func=cmd_logout)

    # whoami command
    whoami_parser = subparsers.add_parser('whoami', help='Show current user and selected instance')
    whoami_parser.set_defaults(func=cmd_whoami)

    # instance command with subcommands
    instance_parser = subparsers.add_parser('instance', help='Manage or select connected API instances')
    instance_subparsers = instance_parser.add_subparsers(dest='instance_command', help='Instance operations')

    instance_add = instance_subparsers.add_parser('add', help='Add a new API instance')
    instance_add.add_argument('name', help='Instance name')
    instance_add.add_argument('url', help='API endpoint URL')
    instance_add.add_argument('--token', help='auth token', type=Union[str, None],
                              default=None, required=False)
    instance_add.add_argument('-d', '--set-default', help='directly set this as default',
                              action='store_true')
    instance_add.set_defaults(func=cmd_instance_add)

    instance_list = instance_subparsers.add_parser('list', aliases=['ls'], help='List all configured instances')
    instance_list.set_defaults(func=cmd_instance_list)

    instance_select = instance_subparsers.add_parser('select', help='Select an instance to use')
    instance_select.add_argument('name', help='Instance name to select')
    instance_select.set_defaults(func=cmd_instance_select)

    instance_remove = instance_subparsers.add_parser('remove', help='Remove an instance')
    instance_remove.add_argument('name', help='Instance name to remove')
    instance_remove.set_defaults(func=cmd_instance_remove)

    # project command
    project_parser = subparsers.add_parser('project', aliases=['p'], help='Manage projects (list, select, create)')
    project_subparsers = project_parser.add_subparsers(dest='project_command', help='Project operations')

    project_list = project_subparsers.add_parser('list', aliases=['ls'], help='List all projects')
    project_list.set_defaults(func=cmd_project_list)

    project_select = project_subparsers.add_parser('select', help='Select a project')
    project_select.add_argument('project_id', help='Project ID to select')
    project_select.set_defaults(func=cmd_project_select)

    project_create = project_subparsers.add_parser('create', help='Create a new project')
    project_create.add_argument('name', help='Project name')
    project_create.add_argument('--description', help='Project description')
    project_create.set_defaults(func=cmd_project_create)

    # collection command
    collection_parser = subparsers.add_parser('collection', aliases=['c'],
                                              help='Manage collections (list, create, describe)')
    collection_subparsers = collection_parser.add_subparsers(dest='collection_command', help='Collection operations')

    collection_list = collection_subparsers.add_parser('list', aliases=['ls'], help='List all collections')
    collection_list.set_defaults(func=cmd_collection_list)
    # collection_list.add_argument('--project', '-p', help='Project ID')

    collection_create = collection_subparsers.add_parser('new', help='Create a new collection')
    collection_create.add_argument('name', help='Collection name')
    collection_create.add_argument('--description', help='Collection description')
    collection_create.set_defaults(func=cmd_collection_new)

    # dataset command
    dataset_parser = subparsers.add_parser('dataset', aliases=['ds'],
                                           help='Manage datasets (upload, list, describe)')
    dataset_subparsers = dataset_parser.add_subparsers(dest='dataset_command', help='Dataset operations')

    dataset_upload = dataset_subparsers.add_parser('upload', aliases=['u'], help='Upload a dataset')
    dataset_upload.add_argument('path', help='Path to dataset file or directory')
    dataset_upload.add_argument('--name', help='Dataset name')
    dataset_upload.add_argument('--collection', help='Collection ID')
    dataset_upload.set_defaults(func=cmd_dataset_upload)

    dataset_download = dataset_subparsers.add_parser('download', aliases=['d', 'get'], help='Download a dataset')
    dataset_download.add_argument('dataset_id', help='Dataset ID')
    dataset_download.add_argument('--output', '-o', help='Output path')
    dataset_download.set_defaults(func=cmd_dataset_download)

    dataset_upload = dataset_subparsers.add_parser('new', help='Create a new dataset, but do not upload anything')
    dataset_upload.add_argument('--name', '-n', help='Dataset name')
    dataset_upload.add_argument('--collection', '-c', help='Collection ID')
    dataset_upload.set_defaults(func=cmd_dataset_upload)

    dataset_seal = dataset_subparsers.add_parser('seal', help='Seal a dataset (make immutable)')
    dataset_seal.add_argument('dataset_id', help='Dataset ID to seal')
    dataset_seal.set_defaults(func=cmd_dataset_seal)

    dataset_metadata = dataset_subparsers.add_parser('metadata', help='Show dataset metadata')
    dataset_metadata.add_argument('dataset_id', help='Dataset ID')
    dataset_metadata.set_defaults(func=cmd_dataset_metadata)

    dataset_list = dataset_subparsers.add_parser('list', aliases=['ls'], help='List all datasets')
    dataset_list.add_argument('--collection', '-c', help='Query in Collection with ID')
    dataset_list.set_defaults(func=cmd_dataset_list)

    dataset_describe = dataset_subparsers.add_parser('describe', help='Describe a dataset')
    dataset_describe.add_argument('dataset_id', help='Dataset ID')
    dataset_describe.set_defaults(func=cmd_dataset_describe)

    # metadata command
    metadata_parser = subparsers.add_parser('metadata', help='Inspect or modify metadata')
    metadata_subparsers = metadata_parser.add_subparsers(dest='metadata_command', help='Metadata operations')

    metadata_show = metadata_subparsers.add_parser('show', help='Show metadata')
    metadata_show.add_argument('resource_id', help='Resource ID')
    metadata_show.add_argument('--type', choices=['project', 'collection', 'dataset'], help='Resource type')
    metadata_show.set_defaults(func=cmd_metadata_show)

    metadata_set = metadata_subparsers.add_parser('set', help='Set metadata value')
    metadata_set.add_argument('resource_id', help='Resource ID')
    metadata_set.add_argument('key', help='Metadata key')
    metadata_set.add_argument('value', help='Metadata value')
    metadata_set.add_argument('--type', choices=['project', 'collection', 'dataset'], help='Resource type')
    metadata_set.set_defaults(func=cmd_metadata_set)

    metadata_validate = metadata_subparsers.add_parser('validate', help='Validate metadata against schema')
    metadata_validate.add_argument('resource_id', help='Resource ID')
    metadata_validate.add_argument('--schema', help='Schema file path')
    metadata_validate.set_defaults(func=cmd_metadata_validate)

    # pipeline command (future)
    pipeline_parser = subparsers.add_parser('pipeline', help='(future) Run or inspect pipeline jobs')
    pipeline_subparsers = pipeline_parser.add_subparsers(dest='pipeline_command', help='Pipeline operations')

    pipeline_run = pipeline_subparsers.add_parser('run', help='Run a pipeline')
    pipeline_run.add_argument('pipeline_id', help='Pipeline ID')
    pipeline_run.set_defaults(func=cmd_pipeline_run)

    pipeline_list = pipeline_subparsers.add_parser('list', aliases=['ls'], help='List pipelines')
    pipeline_list.set_defaults(func=cmd_pipeline_list)

    pipeline_status = pipeline_subparsers.add_parser('status', help='Check pipeline status')
    pipeline_status.add_argument('job_id', help='Job ID')
    pipeline_status.set_defaults(func=cmd_pipeline_status)

    return parser


# Dummy command implementations

def cmd_login(args):
    print(f"[login] Authenticating user: {args.username}")


def cmd_logout(args):
    print("[logout] Logging out and removing token")


def cmd_whoami(args):
    print("[whoami] Showing current user and instance")



def main() -> None:
    parser = build_parser()
    args = parser.parse_args()

    if hasattr(args, 'func'):
        args.func(args)
    else:
        parser.print_help()


if __name__ == '__main__':
    main()
