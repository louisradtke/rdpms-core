# DataStoreSummaryDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** |  | [optional] 
**slug** | **str** |  | [optional] 
**name** | **str** |  | [optional] 
**files_count** | **int** |  | [optional] 
**storage_type** | **str** |  | [optional] 
**properties_json** | **str** |  | [optional] 
**project_id** | **str** |  | [optional] 

## Example

```python
from openapi_client.models.data_store_summary_dto import DataStoreSummaryDTO

# TODO update the JSON string below
json = "{}"
# create an instance of DataStoreSummaryDTO from a JSON string
data_store_summary_dto_instance = DataStoreSummaryDTO.from_json(json)
# print the JSON string representation of the object
print(DataStoreSummaryDTO.to_json())

# convert the object into a dict
data_store_summary_dto_dict = data_store_summary_dto_instance.to_dict()
# create an instance of DataStoreSummaryDTO from a dict
data_store_summary_dto_from_dict = DataStoreSummaryDTO.from_dict(data_store_summary_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


