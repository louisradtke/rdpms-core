# StoresApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataStoresGet**](StoresApi.md#apiv1datastoresget) | **GET** /api/v1/data/stores | Get all data stores. |
| [**apiV1DataStoresIdGet**](StoresApi.md#apiv1datastoresidget) | **GET** /api/v1/data/stores/{id} | Get a single data store by id. |



## apiV1DataStoresGet

> Array&lt;DataStoreSummaryDTO&gt; apiV1DataStoresGet(type, parentProjectId)

Get all data stores.

### Example

```ts
import {
  Configuration,
  StoresApi,
} from '';
import type { ApiV1DataStoresGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new StoresApi();

  const body = {
    // string (optional)
    type: type_example,
    // string (optional)
    parentProjectId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataStoresGetRequest;

  try {
    const data = await api.apiV1DataStoresGet(body);
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
| **type** | `string` |  | [Optional] [Defaults to `undefined`] |
| **parentProjectId** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;DataStoreSummaryDTO&gt;**](DataStoreSummaryDTO.md)

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


## apiV1DataStoresIdGet

> DataStoreSummaryDTO apiV1DataStoresIdGet(id)

Get a single data store by id.

### Example

```ts
import {
  Configuration,
  StoresApi,
} from '';
import type { ApiV1DataStoresIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new StoresApi();

  const body = {
    // string | 
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataStoresIdGetRequest;

  try {
    const data = await api.apiV1DataStoresIdGet(body);
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

[**DataStoreSummaryDTO**](DataStoreSummaryDTO.md)

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

