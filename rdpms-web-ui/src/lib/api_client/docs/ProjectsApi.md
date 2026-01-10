# ProjectsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1ProjectsGet**](ProjectsApi.md#apiv1projectsget) | **GET** /api/v1/projects | Get all projects. |
| [**apiV1ProjectsIdGet**](ProjectsApi.md#apiv1projectsidget) | **GET** /api/v1/projects/{id} |  |
| [**apiV1ProjectsIdPut**](ProjectsApi.md#apiv1projectsidput) | **PUT** /api/v1/projects/{id} | Id field on the body is ignored. |



## apiV1ProjectsGet

> Array&lt;ProjectSummaryDTO&gt; apiV1ProjectsGet(slug)

Get all projects.

### Example

```ts
import {
  Configuration,
  ProjectsApi,
} from '';
import type { ApiV1ProjectsGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ProjectsApi();

  const body = {
    // string (optional)
    slug: slug_example,
  } satisfies ApiV1ProjectsGetRequest;

  try {
    const data = await api.apiV1ProjectsGet(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **slug** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;ProjectSummaryDTO&gt;**](ProjectSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1ProjectsIdGet

> ProjectSummaryDTO apiV1ProjectsIdGet(id)



### Example

```ts
import {
  Configuration,
  ProjectsApi,
} from '';
import type { ApiV1ProjectsIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ProjectsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1ProjectsIdGetRequest;

  try {
    const data = await api.apiV1ProjectsIdGet(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **id** | `string` |  | [Defaults to `undefined`] |

### Return type

[**ProjectSummaryDTO**](ProjectSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1ProjectsIdPut

> apiV1ProjectsIdPut(id, projectSummaryDTO)

Id field on the body is ignored.

### Example

```ts
import {
  Configuration,
  ProjectsApi,
} from '';
import type { ApiV1ProjectsIdPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ProjectsApi();

  const body = {
    // string | 
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // ProjectSummaryDTO |  (optional)
    projectSummaryDTO: ...,
  } satisfies ApiV1ProjectsIdPutRequest;

  try {
    const data = await api.apiV1ProjectsIdPut(body);
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters


| Name | Type | Description  | Notes |
|------------- | ------------- | ------------- | -------------|
| **id** | `string` |  | [Defaults to `undefined`] |
| **projectSummaryDTO** | [ProjectSummaryDTO](ProjectSummaryDTO.md) |  | [Optional] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

