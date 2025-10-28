from __future__ import annotations

from dataclasses import dataclass, asdict
from typing import Union

import yaml
import os
from pathlib import Path


# check if windows:
if os.name == 'nt':
    CONFIG_PATH = Path(os.environ['APPDATA'], 'rdpms', 'config.yaml')
else:
    CONFIG_PATH = Path(os.environ['HOME'], '.config', 'rdpms', 'config.yaml')


@dataclass
class RdpmsInstance:
    # name: str
    base_url: str
    token: str

@dataclass
class ConfigState:
    instances: dict[str, RdpmsInstance]
    active_instance_key: Union[str, None]
    
    @property
    def active_instance(self) -> RdpmsInstance:
        return self.instances[self.active_instance_key]
    
    def build_yaml(self) -> str:
        dct = {
            'active_instance': self.active_instance_key,
            'instances':{
                k: asdict(v)
                for k, v in self.instances.items()
            }
        }
        return yaml.dump(dct)

    @staticmethod
    def parse_yaml(yaml_str: str) -> ConfigState:
        dct = yaml.safe_load(yaml_str)

        match dct:
            case {
                'active_instance': str() as active_instance,
                'instances': dict() as instances_dict
            }:
                instances = ConfigState.__parse_instances(instances_dict)
                return ConfigState(
                    active_instance_key=active_instance,
                    instances=instances
                )
            case {
                'instances': dict() as instances_dict
            }:
                # No active instance specified
                instances = ConfigState.__parse_instances(instances_dict)
                return ConfigState(
                    active_instance_key=next(iter(instances.keys())),
                    instances=instances
                )
            case _:
                raise ValueError("Invalid config format: missing 'instances' field")

    @staticmethod
    def __parse_instances(instances_dict: dict) -> dict[str, RdpmsInstance]:
        """Parse the instances dictionary from the YAML config."""
        result = {}

        for name, config in instances_dict.items():
            match config:
                case {
                    'base_url': str() as base_url,
                    'token': str() as token
                }:
                    result[name] = RdpmsInstance(
                        base_url=base_url,
                        token=token
                    )
                case {
                    'base_url': str() as base_url
                }:
                    # Token is optional
                    result[name] = RdpmsInstance(
                        base_url=base_url,
                        token=None
                    )
                case _:
                    raise ValueError(f"Invalid instance config for '{name}': missing 'base_url'")

        return result
def store_file(state: ConfigState) -> None:
    if not os.path.exists(os.path.dirname(CONFIG_PATH)):
        os.mkdir(os.path.dirname(CONFIG_PATH))
    with open(CONFIG_PATH, 'w') as f:
        f.write(state.build_yaml())
        
def load_file() -> ConfigState:
    if not os.path.exists(CONFIG_PATH):
        return ConfigState(active_instance_key=None, instances={})

    with open(CONFIG_PATH, 'r') as f:
        return ConfigState.parse_yaml(f.read())
