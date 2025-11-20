from rdpms_cli.openapi_client import ApiClient, ContentTypeDTO
from rdpms_cli.openapi_client import ContentTypesApi

class TypeStore:
    def __init__(self, types: list[ContentTypeDTO]):
        self.types = types or []
    
    def resolve_by_ending(self, file_name: str) -> ContentTypeDTO:
        """
        Resolves a file type based on it's ending/suffix.
        :param file_name: the full name of the file
        :return: a ContentTypeDTO, containing the probably relevant type id
        """
        
        for t in self.types:
            if file_name.endswith(f'.{t.abbreviation}'):
                return t
        
        raise Exception(f'Could not resolve type for file: {file_name}')


def get_types(p_id: str, client: ApiClient) -> TypeStore:
    t_api = ContentTypesApi(client)
    t_api.api_v1_data_content_types_get()
    
    return TypeStore(t_api.api_v1_data_content_types_get())
