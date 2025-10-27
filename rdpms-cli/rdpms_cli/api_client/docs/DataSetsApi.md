# openapi_client.DataSetsApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_data_datasets_batch_post**](DataSetsApi.md#api_v1_data_datasets_batch_post) | **POST** /api/v1/data/datasets/batch | Add a batch of item to the system.
[**api_v1_data_datasets_get**](DataSetsApi.md#api_v1_data_datasets_get) | **GET** /api/v1/data/datasets | 
[**api_v1_data_datasets_id_add_s3_post**](DataSetsApi.md#api_v1_data_datasets_id_add_s3_post) | **POST** /api/v1/data/datasets/{id}/add/s3 | Add a single file to the system. Request a single S3 upload URL.
[**api_v1_data_datasets_id_get**](DataSetsApi.md#api_v1_data_datasets_id_get) | **GET** /api/v1/data/datasets/{id} | 
[**api_v1_data_datasets_id_seal_put**](DataSetsApi.md#api_v1_data_datasets_id_seal_put) | **PUT** /api/v1/data/datasets/{id}/seal | Seals a data set. Only works for data sets that are in \&quot;Uninitialized\&quot; state.
[**api_v1_data_datasets_post**](DataSetsApi.md#api_v1_data_datasets_post) | **POST** /api/v1/data/datasets | Add a single item to the system.


# **api_v1_data_datasets_batch_post**
> api_v1_data_datasets_batch_post(data_set_summary_dto=data_set_summary_dto)

Add a batch of item to the system.

### Example


```python
import openapi_client
from openapi_client.models.data_set_summary_dto import DataSetSummaryDTO
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
    api_instance = openapi_client.DataSetsApi(api_client)
    data_set_summary_dto = [openapi_client.DataSetSummaryDTO()] # List[DataSetSummaryDTO] |  (optional)

    try:
        # Add a batch of item to the system.
        api_instance.api_v1_data_datasets_batch_post(data_set_summary_dto=data_set_summary_dto)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_batch_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **data_set_summary_dto** | [**List[DataSetSummaryDTO]**](DataSetSummaryDTO.md)|  | [optional] 

### Return type

void (empty response body)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: Not defined

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_datasets_get**
> List[DataSetSummaryDTO] api_v1_data_datasets_get(collection_id=collection_id)



### Example


```python
import openapi_client
from openapi_client.models.data_set_summary_dto import DataSetSummaryDTO
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
    api_instance = openapi_client.DataSetsApi(api_client)
    collection_id = 'collection_id_example' # str |  (optional)

    try:
        api_response = api_instance.api_v1_data_datasets_get(collection_id=collection_id)
        print("The response of DataSetsApi->api_v1_data_datasets_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **collection_id** | **str**|  | [optional] 

### Return type

[**List[DataSetSummaryDTO]**](DataSetSummaryDTO.md)

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

# **api_v1_data_datasets_id_add_s3_post**
> FileCreateResponseDTO api_v1_data_datasets_id_add_s3_post(id, s3_file_create_request_dto=s3_file_create_request_dto)

Add a single file to the system. Request a single S3 upload URL.

### Example


```python
import openapi_client
from openapi_client.models.file_create_response_dto import FileCreateResponseDTO
from openapi_client.models.s3_file_create_request_dto import S3FileCreateRequestDTO
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
    api_instance = openapi_client.DataSetsApi(api_client)
    id = 'id_example' # str | 
    s3_file_create_request_dto = openapi_client.S3FileCreateRequestDTO() # S3FileCreateRequestDTO |  (optional)

    try:
        # Add a single file to the system. Request a single S3 upload URL.
        api_response = api_instance.api_v1_data_datasets_id_add_s3_post(id, s3_file_create_request_dto=s3_file_create_request_dto)
        print("The response of DataSetsApi->api_v1_data_datasets_id_add_s3_post:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_id_add_s3_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 
 **s3_file_create_request_dto** | [**S3FileCreateRequestDTO**](S3FileCreateRequestDTO.md)|  | [optional] 

### Return type

[**FileCreateResponseDTO**](FileCreateResponseDTO.md)

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |
**400** | Bad Request |  -  |
**404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_datasets_id_get**
> DataSetDetailedDTO api_v1_data_datasets_id_get(id)



### Example


```python
import openapi_client
from openapi_client.models.data_set_detailed_dto import DataSetDetailedDTO
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
    api_instance = openapi_client.DataSetsApi(api_client)
    id = 'id_example' # str | 

    try:
        api_response = api_instance.api_v1_data_datasets_id_get(id)
        print("The response of DataSetsApi->api_v1_data_datasets_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 

### Return type

[**DataSetDetailedDTO**](DataSetDetailedDTO.md)

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

# **api_v1_data_datasets_id_seal_put**
> api_v1_data_datasets_id_seal_put(id)

Seals a data set. Only works for data sets that are in \"Uninitialized\" state.

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
    api_instance = openapi_client.DataSetsApi(api_client)
    id = 'id_example' # str | The data set id.

    try:
        # Seals a data set. Only works for data sets that are in \"Uninitialized\" state.
        api_instance.api_v1_data_datasets_id_seal_put(id)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_id_seal_put: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**| The data set id. | 

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
**200** | OK |  -  |
**404** | Not Found |  -  |
**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_datasets_post**
> str api_v1_data_datasets_post(data_set_summary_dto=data_set_summary_dto)

Add a single item to the system.

### Example


```python
import openapi_client
from openapi_client.models.data_set_summary_dto import DataSetSummaryDTO
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
    api_instance = openapi_client.DataSetsApi(api_client)
    data_set_summary_dto = openapi_client.DataSetSummaryDTO() # DataSetSummaryDTO |  (optional)

    try:
        # Add a single item to the system.
        api_response = api_instance.api_v1_data_datasets_post(data_set_summary_dto=data_set_summary_dto)
        print("The response of DataSetsApi->api_v1_data_datasets_post:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling DataSetsApi->api_v1_data_datasets_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **data_set_summary_dto** | [**DataSetSummaryDTO**](DataSetSummaryDTO.md)|  | [optional] 

### Return type

**str**

### Authorization

No authorization required

### HTTP request headers

 - **Content-Type**: application/json
 - **Accept**: application/json

### HTTP response details

| Status code | Description | Response headers |
|-------------|-------------|------------------|
**200** | OK |  -  |
**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

