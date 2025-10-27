# openapi_client.CollectionsApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_data_collections_get**](CollectionsApi.md#api_v1_data_collections_get) | **GET** /api/v1/data/collections | Get all collections.
[**api_v1_data_collections_id_get**](CollectionsApi.md#api_v1_data_collections_id_get) | **GET** /api/v1/data/collections/{id} | Get a single collection by id.
[**api_v1_data_collections_post**](CollectionsApi.md#api_v1_data_collections_post) | **POST** /api/v1/data/collections | Add a single item to the system.


# **api_v1_data_collections_get**
> List[CollectionSummaryDTO] api_v1_data_collections_get(project_id=project_id, project_slug=project_slug, slug=slug)

Get all collections.

### Example


```python
import openapi_client
from openapi_client.models.collection_summary_dto import CollectionSummaryDTO
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
    api_instance = openapi_client.CollectionsApi(api_client)
    project_id = 'project_id_example' # str | Used to filter for collections with this parent project (optional)
    project_slug = 'project_slug_example' # str | Used to filter for collections with this parent project (optional)
    slug = 'slug_example' # str | Used to filter for collections with this slug (optional)

    try:
        # Get all collections.
        api_response = api_instance.api_v1_data_collections_get(project_id=project_id, project_slug=project_slug, slug=slug)
        print("The response of CollectionsApi->api_v1_data_collections_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling CollectionsApi->api_v1_data_collections_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **project_id** | **str**| Used to filter for collections with this parent project | [optional] 
 **project_slug** | **str**| Used to filter for collections with this parent project | [optional] 
 **slug** | **str**| Used to filter for collections with this slug | [optional] 

### Return type

[**List[CollectionSummaryDTO]**](CollectionSummaryDTO.md)

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

# **api_v1_data_collections_id_get**
> CollectionSummaryDTO api_v1_data_collections_id_get(id)

Get a single collection by id.

### Example


```python
import openapi_client
from openapi_client.models.collection_summary_dto import CollectionSummaryDTO
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
    api_instance = openapi_client.CollectionsApi(api_client)
    id = 'id_example' # str | 

    try:
        # Get a single collection by id.
        api_response = api_instance.api_v1_data_collections_id_get(id)
        print("The response of CollectionsApi->api_v1_data_collections_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling CollectionsApi->api_v1_data_collections_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 

### Return type

[**CollectionSummaryDTO**](CollectionSummaryDTO.md)

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

# **api_v1_data_collections_post**
> api_v1_data_collections_post(collection_summary_dto=collection_summary_dto)

Add a single item to the system.

### Example


```python
import openapi_client
from openapi_client.models.collection_summary_dto import CollectionSummaryDTO
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
    api_instance = openapi_client.CollectionsApi(api_client)
    collection_summary_dto = openapi_client.CollectionSummaryDTO() # CollectionSummaryDTO |  (optional)

    try:
        # Add a single item to the system.
        api_instance.api_v1_data_collections_post(collection_summary_dto=collection_summary_dto)
    except Exception as e:
        print("Exception when calling CollectionsApi->api_v1_data_collections_post: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **collection_summary_dto** | [**CollectionSummaryDTO**](CollectionSummaryDTO.md)|  | [optional] 

### Return type

void (empty response body)

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

