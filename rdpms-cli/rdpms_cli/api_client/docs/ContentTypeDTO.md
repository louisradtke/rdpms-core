# ContentTypeDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** |  | [optional] 
**abbreviation** | **str** |  | [optional] 
**display_name** | **str** |  | [optional] 
**description** | **str** |  | [optional] 
**mime_type** | **str** |  | [optional] 

## Example

```python
from openapi_client.models.content_type_dto import ContentTypeDTO

# TODO update the JSON string below
json = "{}"
# create an instance of ContentTypeDTO from a JSON string
content_type_dto_instance = ContentTypeDTO.from_json(json)
# print the JSON string representation of the object
print(ContentTypeDTO.to_json())

# convert the object into a dict
content_type_dto_dict = content_type_dto_instance.to_dict()
# create an instance of ContentTypeDTO from a dict
content_type_dto_from_dict = ContentTypeDTO.from_dict(content_type_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


