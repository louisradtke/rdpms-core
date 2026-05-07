# ExecutionsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1ExecutionsFinishedPost**](ExecutionsApi.md#apiv1executionsfinishedpost) | **POST** /api/v1/executions/finished | Register a finished execution and its direct source/output artifacts. |
| [**apiV1ExecutionsGet**](ExecutionsApi.md#apiv1executionsget) | **GET** /api/v1/executions | Get all executions, optionally filtered by the dataset they produced or consumed. |
| [**apiV1ExecutionsIdGet**](ExecutionsApi.md#apiv1executionsidget) | **GET** /api/v1/executions/{id} | Get a single execution by id. |



## apiV1ExecutionsFinishedPost

> ExecutionSummaryDTO apiV1ExecutionsFinishedPost(executionFinishedRequestDTO)

Register a finished execution and its direct source/output artifacts.

### Example

```ts
import {
  Configuration,
  ExecutionsApi,
} from '';
import type { ApiV1ExecutionsFinishedPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ExecutionsApi();

  const body = {
    // ExecutionFinishedRequestDTO (optional)
    executionFinishedRequestDTO: ...,
  } satisfies ApiV1ExecutionsFinishedPostRequest;

  try {
    const data = await api.apiV1ExecutionsFinishedPost(body);
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
| **executionFinishedRequestDTO** | [ExecutionFinishedRequestDTO](ExecutionFinishedRequestDTO.md) |  | [Optional] |

### Return type

[**ExecutionSummaryDTO**](ExecutionSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1ExecutionsGet

> Array&lt;ExecutionSummaryDTO&gt; apiV1ExecutionsGet(ancestorOf, childOf)

Get all executions, optionally filtered by the dataset they produced or consumed.

### Example

```ts
import {
  Configuration,
  ExecutionsApi,
} from '';
import type { ApiV1ExecutionsGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ExecutionsApi();

  const body = {
    // string (optional)
    ancestorOf: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    childOf: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1ExecutionsGetRequest;

  try {
    const data = await api.apiV1ExecutionsGet(body);
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
| **ancestorOf** | `string` |  | [Optional] [Defaults to `undefined`] |
| **childOf** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;ExecutionSummaryDTO&gt;**](ExecutionSummaryDTO.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1ExecutionsIdGet

> ExecutionSummaryDTO apiV1ExecutionsIdGet(id)

Get a single execution by id.

### Example

```ts
import {
  Configuration,
  ExecutionsApi,
} from '';
import type { ApiV1ExecutionsIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ExecutionsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1ExecutionsIdGetRequest;

  try {
    const data = await api.apiV1ExecutionsIdGet(body);
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

[**ExecutionSummaryDTO**](ExecutionSummaryDTO.md)

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

