# CollectionSummaryDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** |  | [optional] 
**slug** | **str** |  | [optional] 
**name** | **str** |  | [optional] 
**description** | **str** |  | [optional] 
**data_set_count** | **int** |  | [optional] 
**default_data_store_id** | **str** |  | [optional] 
**project_id** | **str** |  | [optional] 

## Example

```python
from openapi_client.models.collection_summary_dto import CollectionSummaryDTO

# TODO update the JSON string below
json = "{}"
# create an instance of CollectionSummaryDTO from a JSON string
collection_summary_dto_instance = CollectionSummaryDTO.from_json(json)
# print the JSON string representation of the object
print(CollectionSummaryDTO.to_json())

# convert the object into a dict
collection_summary_dto_dict = collection_summary_dto_instance.to_dict()
# create an instance of CollectionSummaryDTO from a dict
collection_summary_dto_from_dict = CollectionSummaryDTO.from_dict(collection_summary_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


