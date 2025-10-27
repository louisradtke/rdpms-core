# ErrorMessageDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**message** | **str** | The error message. This must be set. If the \&quot;user friendly\&quot; RDPMS.Core.Server.Model.DTO.V1.ErrorMessageDTO.DisplayMessage is set, this may become technical. | [optional] 
**display_message** | **str** | A message dedicated to the user. If null, use RDPMS.Core.Server.Model.DTO.V1.ErrorMessageDTO.Message as fallback. | [optional] 

## Example

```python
from openapi_client.models.error_message_dto import ErrorMessageDTO

# TODO update the JSON string below
json = "{}"
# create an instance of ErrorMessageDTO from a JSON string
error_message_dto_instance = ErrorMessageDTO.from_json(json)
# print the JSON string representation of the object
print(ErrorMessageDTO.to_json())

# convert the object into a dict
error_message_dto_dict = error_message_dto_instance.to_dict()
# create an instance of ErrorMessageDTO from a dict
error_message_dto_from_dict = ErrorMessageDTO.from_dict(error_message_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


