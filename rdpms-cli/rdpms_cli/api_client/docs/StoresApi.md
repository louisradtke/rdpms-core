# openapi_client.StoresApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_data_stores_get**](StoresApi.md#api_v1_data_stores_get) | **GET** /api/v1/data/stores | Get all data stores.
[**api_v1_data_stores_id_get**](StoresApi.md#api_v1_data_stores_id_get) | **GET** /api/v1/data/stores/{id} | Get a single data store by id.


# **api_v1_data_stores_get**
> List[DataStoreSummaryDTO] api_v1_data_stores_get(type=type)

Get all data stores.

### Example


```python
import openapi_client
from openapi_client.models.data_store_summary_dto import DataStoreSummaryDTO
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
    api_instance = openapi_client.StoresApi(api_client)
    type = 'type_example' # str |  (optional)

    try:
        # Get all data stores.
        api_response = api_instance.api_v1_data_stores_get(type=type)
        print("The response of StoresApi->api_v1_data_stores_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling StoresApi->api_v1_data_stores_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **type** | **str**|  | [optional] 

### Return type

[**List[DataStoreSummaryDTO]**](DataStoreSummaryDTO.md)

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

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

# **api_v1_data_stores_id_get**
> DataStoreSummaryDTO api_v1_data_stores_id_get(id)

Get a single data store by id.

### Example


```python
import openapi_client
from openapi_client.models.data_store_summary_dto import DataStoreSummaryDTO
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
    api_instance = openapi_client.StoresApi(api_client)
    id = 'id_example' # str | 

    try:
        # Get a single data store by id.
        api_response = api_instance.api_v1_data_stores_id_get(id)
        print("The response of StoresApi->api_v1_data_stores_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling StoresApi->api_v1_data_stores_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 

### Return type

[**DataStoreSummaryDTO**](DataStoreSummaryDTO.md)

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

