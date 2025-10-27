# DataSetDetailedDTO

Represents a summary of a dataset, including identifying information, timestamps, state, tags, and metadata fields.

## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**id** | **str** | Uniquely identifies the dataset. Typically server-generated. Should not be manually set by the client. | [optional] 
**slug** | **str** | An optional, human-readable identifier for the dataset. | [optional] 
**name** | **str** | Non-unique, mandatory descriptive name of the dataset. Must be provided by the client. | [optional] 
**assigned_tags** | [**List[TagDTO]**](TagDTO.md) | List of tags associated with the dataset, used for categorization and filtering purposes. | [optional] 
**created_stamp_utc** | **datetime** | UTC timestamp when the dataset was originally created. Mandatory property, should be provided by the client. | [optional] 
**deleted_stamp_utc** | **datetime** | UTC timestamp that indicates when the dataset was deleted. Null if the dataset has not been deleted yet. | [optional] 
**begin_stamp_utc** | **datetime** | UTC timestamp that marks the period begin of the dataset. Only to be set by server. | [optional] 
**end_stamp_utc** | **datetime** | UTC timestamp that marks the period end of the dataset. Only to be set by server. | [optional] 
**state** | **str** | Indicates, whether the dataset (and its files) are immutable. Only to be set by server. Lifecycle is: Uninitialized -&gt; [Sealed -&gt;] Deleted | [optional] 
**is_time_series** | **bool** | Indicates if the dataset represents time-series data. Only to be set by server. | [optional] 
**is_deleted** | **bool** | Flags whether the dataset has been marked as deleted. Only to be set by server. | [optional] 
**metadata_fields** | **List[str]** | Fields, for which metadata exists. Only to be set by server. | [optional] 
**file_count** | **int** | Amount of files in the dataset. | [optional] 
**collection_id** | **str** | Id of the collection this dataset belongs to. | [optional] 
**files** | [**List[FileSummaryDTO]**](FileSummaryDTO.md) |  | [optional] 

## Example

```python
from openapi_client.models.data_set_detailed_dto import DataSetDetailedDTO

# TODO update the JSON string below
json = "{}"
# create an instance of DataSetDetailedDTO from a JSON string
data_set_detailed_dto_instance = DataSetDetailedDTO.from_json(json)
# print the JSON string representation of the object
print(DataSetDetailedDTO.to_json())

# convert the object into a dict
data_set_detailed_dto_dict = data_set_detailed_dto_instance.to_dict()
# create an instance of DataSetDetailedDTO from a dict
data_set_detailed_dto_from_dict = DataSetDetailedDTO.from_dict(data_set_detailed_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


