# S3FileCreateRequestDTO


## Properties

Name | Type | Description | Notes
------------ | ------------- | ------------- | -------------
**name** | **str** |  | [optional] 
**content_type_id** | **str** |  | [optional] 
**size_bytes** | **int** |  | [optional] 
**plain_sha256_hash** | **str** |  | [optional] 
**created_stamp** | **datetime** |  | [optional] 
**begin_stamp** | **datetime** |  | [optional] 
**end_stamp** | **datetime** |  | [optional] 
**compression_algorithm** | **str** |  | [optional] 
**compressed_sha256_hash** | **str** | If file is compressed, this is the SHA256 hash of the compressed file. | [optional] 
**compressed_size_bytes** | **int** |  | [optional] 

## Example

```python
from openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO

# TODO update the JSON string below
json = "{}"
# create an instance of S3FileCreateRequestDTO from a JSON string
s3_file_create_request_dto_instance = S3FileCreateRequestDTO.from_json(json)
# print the JSON string representation of the object
print(S3FileCreateRequestDTO.to_json())

# convert the object into a dict
s3_file_create_request_dto_dict = s3_file_create_request_dto_instance.to_dict()
# create an instance of S3FileCreateRequestDTO from a dict
s3_file_create_request_dto_from_dict = S3FileCreateRequestDTO.from_dict(s3_file_create_request_dto_dict)
```
[[Back to Model list]](../README.md#documentation-for-models) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to README]](../README.md)


