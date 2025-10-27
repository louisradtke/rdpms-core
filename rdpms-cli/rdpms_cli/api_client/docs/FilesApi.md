# openapi_client.FilesApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_data_file_refs_get**](FilesApi.md#api_v1_data_file_refs_get) | **GET** /api/v1/data/file-refs | 
[**api_v1_data_files_get**](FilesApi.md#api_v1_data_files_get) | **GET** /api/v1/data/files | Get summaries of all files.
[**api_v1_data_files_id_blob_get**](FilesApi.md#api_v1_data_files_id_blob_get) | **GET** /api/v1/data/files/{id}/blob | Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned.
[**api_v1_data_files_id_content_get**](FilesApi.md#api_v1_data_files_id_content_get) | **GET** /api/v1/data/files/{id}/content | Request redirect to a file. On success, a 302 referring to the final download URL will be returned.
[**api_v1_data_files_id_get**](FilesApi.md#api_v1_data_files_id_get) | **GET** /api/v1/data/files/{id} | Get summary of a single file.


# **api_v1_data_file_refs_get**
> FileSummaryDTO api_v1_data_file_refs_get(store_guid=store_guid, type=type)



### Example


```python
import openapi_client
from openapi_client.models.file_summary_dto import FileSummaryDTO
from openapi_client.rest import ApiException
from pprint import pprint

# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)


# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = openapi_client.FilesApi(api_client)
    store_guid = 'store_guid_example' # str |  (optional)
    type = 'type_example' # str |  (optional)

    try:
        api_response = api_instance.api_v1_data_file_refs_get(store_guid=store_guid, type=type)
        print("The response of FilesApi->api_v1_data_file_refs_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling FilesApi->api_v1_data_file_refs_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **store_guid** | **str**|  | [optional] 
 **type** | **str**|  | [optional] 

### Return type

[**FileSummaryDTO**](FileSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_files_get**
> List[FileSummaryDTO] api_v1_data_files_get()

Get summaries of all files.

### Example


```python
import openapi_client
from openapi_client.models.file_summary_dto import FileSummaryDTO
from openapi_client.rest import ApiException
from pprint import pprint

# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)


# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = openapi_client.FilesApi(api_client)

    try:
        # Get summaries of all files.
        api_response = api_instance.api_v1_data_files_get()
        print("The response of FilesApi->api_v1_data_files_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling FilesApi->api_v1_data_files_get: %s\n" % e)
```



### Parameters

This endpoint does not need any parameter.

### Return type

[**List[FileSummaryDTO]**](FileSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_files_id_blob_get**
> bytearray api_v1_data_files_id_blob_get(id)

Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned.

### Example


```python
import openapi_client
from openapi_client.rest import ApiException
from pprint import pprint

# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)


# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = openapi_client.FilesApi(api_client)
    id = 'id_example' # str | Id of the file to download.

    try:
        # Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned.
        api_response = api_instance.api_v1_data_files_id_blob_get(id)
        print("The response of FilesApi->api_v1_data_files_id_blob_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling FilesApi->api_v1_data_files_id_blob_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**| Id of the file to download. | 

### Return type

**bytearray**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |
**400** | Bad Request |  -  |
**404** | Not Found |  -  |
**501** | Not Implemented |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_files_id_content_get**
> api_v1_data_files_id_content_get(id)

Request redirect to a file. On success, a 302 referring to the final download URL will be returned.

### Example


```python
import openapi_client
from openapi_client.rest import ApiException
from pprint import pprint

# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)


# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = openapi_client.FilesApi(api_client)
    id = 'id_example' # str | Id of the file to download.

    try:
        # Request redirect to a file. On success, a 302 referring to the final download URL will be returned.
        api_instance.api_v1_data_files_id_content_get(id)
    except Exception as e:
        print("Exception when calling FilesApi->api_v1_data_files_id_content_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**| Id of the file to download. | 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**302** | Found |  -  |
**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_files_id_get**
> FileSummaryDTO api_v1_data_files_id_get(id)

Get summary of a single file.

### Example


```python
import openapi_client
from openapi_client.models.file_summary_dto import FileSummaryDTO
from openapi_client.rest import ApiException
from pprint import pprint

# Defining the host is optional and defaults to http://localhost
# See configuration.py for a list of all supported configuration parameters.
configuration = openapi_client.Configuration(
    host = "http://localhost"
)


# Enter a context with an instance of the API client
with openapi_client.ApiClient(configuration) as api_client:
    # Create an instance of the API class
    api_instance = openapi_client.FilesApi(api_client)
    id = 'id_example' # str | Id of the file.

    try:
        # Get summary of a single file.
        api_response = api_instance.api_v1_data_files_id_get(id)
        print("The response of FilesApi->api_v1_data_files_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling FilesApi->api_v1_data_files_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**| Id of the file. | 

### Return type

[**FileSummaryDTO**](FileSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: Not defined
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |
**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

