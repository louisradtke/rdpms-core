# ProjectSummaryDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** | The id of this project. | [optional] 
**slug** | **str** | The slug of this project. | [optional] 
**name** | **str** | The name of this project. | [optional] 
**description** | **str** |  | [optional] 
**collections** | [**List[CollectionSummaryDTO]**](CollectionSummaryDTO.md) | All collections assigned to this project. | [optional] 
**data_stores** | [**List[DataStoreSummaryDTO]**](DataStoreSummaryDTO.md) | All data stores assigned to this project. | [optional] 

## Example

```python
from openapi_client.models.project_summary_dto import ProjectSummaryDTO

# TODO update the JSON string below
json = "{}"
# create an instance of ProjectSummaryDTO from a JSON string
project_summary_dto_instance = ProjectSummaryDTO.from_json(json)
# print the JSON string representation of the object
print(ProjectSummaryDTO.to_json())

# convert the object into a dict
project_summary_dto_dict = project_summary_dto_instance.to_dict()
# create an instance of ProjectSummaryDTO from a dict
project_summary_dto_from_dict = ProjectSummaryDTO.from_dict(project_summary_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


