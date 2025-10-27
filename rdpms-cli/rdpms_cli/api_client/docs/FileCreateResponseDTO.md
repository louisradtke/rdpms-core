# FileCreateResponseDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**upload_uri** | **str** |  | [optional] 
**file_id** | **str** |  | [optional] 

## Example

```python
from openapi_client.models.file_create_response_dto import FileCreateResponseDTO

# TODO update the JSON string below
json = "{}"
# create an instance of FileCreateResponseDTO from a JSON string
file_create_response_dto_instance = FileCreateResponseDTO.from_json(json)
# print the JSON string representation of the object
print(FileCreateResponseDTO.to_json())

# convert the object into a dict
file_create_response_dto_dict = file_create_response_dto_instance.to_dict()
# create an instance of FileCreateResponseDTO from a dict
file_create_response_dto_from_dict = FileCreateResponseDTO.from_dict(file_create_response_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


