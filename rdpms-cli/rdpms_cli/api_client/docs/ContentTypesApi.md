# openapi_client.ContentTypesApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_data_content_types_batch_post**](ContentTypesApi.md#api_v1_data_content_types_batch_post) | **POST** /api/v1/data/content-types/batch | Add a batch of content types to the system.
[**api_v1_data_content_types_get**](ContentTypesApi.md#api_v1_data_content_types_get) | **GET** /api/v1/data/content-types | 
[**api_v1_data_content_types_id_get**](ContentTypesApi.md#api_v1_data_content_types_id_get) | **GET** /api/v1/data/content-types/{id} | 
[**api_v1_data_content_types_post**](ContentTypesApi.md#api_v1_data_content_types_post) | **POST** /api/v1/data/content-types | Add a single content type to the system.


# **api_v1_data_content_types_batch_post**
> api_v1_data_content_types_batch_post(content_type_dto=content_type_dto)

Add a batch of content types to the system.

### Example


```python
import openapi_client
from openapi_client.models.content_type_dto import ContentTypeDTO
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
    api_instance = openapi_client.ContentTypesApi(api_client)
    content_type_dto = [openapi_client.ContentTypeDTO()] # List[ContentTypeDTO] |  (optional)

    try:
        # Add a batch of content types to the system.
        api_instance.api_v1_data_content_types_batch_post(content_type_dto=content_type_dto)
    except Exception as e:
        print("Exception when calling ContentTypesApi->api_v1_data_content_types_batch_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **content_type_dto** | [**List[ContentTypeDTO]**](ContentTypeDTO.md)|  | [optional] 

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

# **api_v1_data_content_types_get**
> List[ContentTypeDTO] api_v1_data_content_types_get()



### Example


```python
import openapi_client
from openapi_client.models.content_type_dto import ContentTypeDTO
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
    api_instance = openapi_client.ContentTypesApi(api_client)

    try:
        api_response = api_instance.api_v1_data_content_types_get()
        print("The response of ContentTypesApi->api_v1_data_content_types_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling ContentTypesApi->api_v1_data_content_types_get: %s\n" % e)
```



### Parameters

This endpoint does not need any parameter.

### Return type

[**List[ContentTypeDTO]**](ContentTypeDTO.md)

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

# **api_v1_data_content_types_id_get**
> ContentTypeDTO api_v1_data_content_types_id_get(id)



### Example


```python
import openapi_client
from openapi_client.models.content_type_dto import ContentTypeDTO
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
    api_instance = openapi_client.ContentTypesApi(api_client)
    id = 'id_example' # str | 

    try:
        api_response = api_instance.api_v1_data_content_types_id_get(id)
        print("The response of ContentTypesApi->api_v1_data_content_types_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling ContentTypesApi->api_v1_data_content_types_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 

### Return type

[**ContentTypeDTO**](ContentTypeDTO.md)

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

# **api_v1_data_content_types_post**
> api_v1_data_content_types_post(content_type_dto=content_type_dto)

Add a single content type to the system.

### Example


```python
import openapi_client
from openapi_client.models.content_type_dto import ContentTypeDTO
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
    api_instance = openapi_client.ContentTypesApi(api_client)
    content_type_dto = openapi_client.ContentTypeDTO() # ContentTypeDTO |  (optional)

    try:
        # Add a single content type to the system.
        api_instance.api_v1_data_content_types_post(content_type_dto=content_type_dto)
    except Exception as e:
        print("Exception when calling ContentTypesApi->api_v1_data_content_types_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **content_type_dto** | [**ContentTypeDTO**](ContentTypeDTO.md)|  | [optional] 

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

