# FileSummaryDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** |  | [optional] 
**name** | **str** |  | [optional] 
**download_uri** | **str** |  | [optional] 
**content_type** | [**ContentTypeDTO**](ContentTypeDTO.md) |  | [optional] 
**size** | **int** |  | [optional] 
**created_stamp_utc** | **datetime** |  | [optional] 
**deleted_stamp_utc** | **datetime** |  | [optional] 
**begin_stamp_utc** | **datetime** |  | [optional] 
**end_stamp_utc** | **datetime** |  | [optional] 
**is_time_series** | **bool** |  | [optional] 
**is_deleted** | **bool** |  | [optional] 

## Example

```python
from openapi_client.models.file_summary_dto import FileSummaryDTO

# TODO update the JSON string below
json = "{}"
# create an instance of FileSummaryDTO from a JSON string
file_summary_dto_instance = FileSummaryDTO.from_json(json)
# print the JSON string representation of the object
print(FileSummaryDTO.to_json())

# convert the object into a dict
file_summary_dto_dict = file_summary_dto_instance.to_dict()
# create an instance of FileSummaryDTO from a dict
file_summary_dto_from_dict = FileSummaryDTO.from_dict(file_summary_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


