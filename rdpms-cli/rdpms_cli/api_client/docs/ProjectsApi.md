# openapi_client.ProjectsApi

All URIs are relative to *http://localhost*

Method | HTTP request | Description
------------- | ------------- | -------------
[**api_v1_projects_get**](ProjectsApi.md#api_v1_projects_get) | **GET** /api/v1/projects | Get all projects.
[**api_v1_projects_id_get**](ProjectsApi.md#api_v1_projects_id_get) | **GET** /api/v1/projects/{id} | 
[**api_v1_projects_id_put**](ProjectsApi.md#api_v1_projects_id_put) | **PUT** /api/v1/projects/{id} | Id field on the body is ignored.


# **api_v1_projects_get**
> List[ProjectSummaryDTO] api_v1_projects_get(slug=slug)

Get all projects.

### Example


```python
import openapi_client
from openapi_client.models.project_summary_dto import ProjectSummaryDTO
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
    api_instance = openapi_client.ProjectsApi(api_client)
    slug = 'slug_example' # str |  (optional)

    try:
        # Get all projects.
        api_response = api_instance.api_v1_projects_get(slug=slug)
        print("The response of ProjectsApi->api_v1_projects_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling ProjectsApi->api_v1_projects_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **slug** | **str**|  | [optional] 

### Return type

[**List[ProjectSummaryDTO]**](ProjectSummaryDTO.md)

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

# **api_v1_projects_id_get**
> ProjectSummaryDTO api_v1_projects_id_get(id)



### Example


```python
import openapi_client
from openapi_client.models.project_summary_dto import ProjectSummaryDTO
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
    api_instance = openapi_client.ProjectsApi(api_client)
    id = 'id_example' # str | 

    try:
        api_response = api_instance.api_v1_projects_id_get(id)
        print("The response of ProjectsApi->api_v1_projects_id_get:\n")
        pprint(api_response)
    except Exception as e:
        print("Exception when calling ProjectsApi->api_v1_projects_id_get: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 

### Return type

[**ProjectSummaryDTO**](ProjectSummaryDTO.md)

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

# **api_v1_projects_id_put**
> api_v1_projects_id_put(id, project_summary_dto=project_summary_dto)

Id field on the body is ignored.

### Example


```python
import openapi_client
from openapi_client.models.project_summary_dto import ProjectSummaryDTO
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
    api_instance = openapi_client.ProjectsApi(api_client)
    id = 'id_example' # str | 
    project_summary_dto = openapi_client.ProjectSummaryDTO() # ProjectSummaryDTO |  (optional)

    try:
        # Id field on the body is ignored.
        api_instance.api_v1_projects_id_put(id, project_summary_dto=project_summary_dto)
    except Exception as e:
        print("Exception when calling ProjectsApi->api_v1_projects_id_put: %s\n" % e)
```



### Parameters


Name | Type | Description  | Notes
------------- | ------------- | ------------- | -------------
 **id** | **str**|  | 
 **project_summary_dto** | [**ProjectSummaryDTO**](ProjectSummaryDTO.md)|  | [optional] 

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
**204** | No Content |  -  |
**400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#documentation-for-api-endpoints) [[Back to Model list]](../README.md#documentation-for-models) [[Back to README]](../README.md)

