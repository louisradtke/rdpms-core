# ContentTypesApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataContentTypesBatchPost**](ContentTypesApi.md#apiv1datacontenttypesbatchpost) | **POST** /api/v1/data/content-types/batch | Add a batch of content types to the system. |
| [**apiV1DataContentTypesGet**](ContentTypesApi.md#apiv1datacontenttypesget) | **GET** /api/v1/data/content-types |  |
| [**apiV1DataContentTypesIdGet**](ContentTypesApi.md#apiv1datacontenttypesidget) | **GET** /api/v1/data/content-types/{id} |  |
| [**apiV1DataContentTypesPost**](ContentTypesApi.md#apiv1datacontenttypespost) | **POST** /api/v1/data/content-types | Add a single content type to the system. |



## apiV1DataContentTypesBatchPost

> apiV1DataContentTypesBatchPost(contentTypeDTO)

Add a batch of content types to the system.

### Example

```ts
import {
  Configuration,
  ContentTypesApi,
} from '';
import type { ApiV1DataContentTypesBatchPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ContentTypesApi();

  const body = {
    // Array<ContentTypeDTO> |  (optional)
    contentTypeDTO: ...,
  } satisfies ApiV1DataContentTypesBatchPostRequest;

  try {
    const data = await api.apiV1DataContentTypesBatchPost(body);
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
| **contentTypeDTO** | `Array<ContentTypeDTO>` |  | [Optional] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataContentTypesGet

> Array&lt;ContentTypeDTO&gt; apiV1DataContentTypesGet()



### Example

```ts
import {
  Configuration,
  ContentTypesApi,
} from '';
import type { ApiV1DataContentTypesGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ContentTypesApi();

  try {
    const data = await api.apiV1DataContentTypesGet();
    console.log(data);
  } catch (error) {
    console.error(error);
  }
}

// Run the test
example().catch(console.error);
```

### Parameters

This endpoint does not need any parameter.

### Return type

[**Array&lt;ContentTypeDTO&gt;**](ContentTypeDTO.md)

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


## apiV1DataContentTypesIdGet

> ContentTypeDTO apiV1DataContentTypesIdGet(id)



### Example

```ts
import {
  Configuration,
  ContentTypesApi,
} from '';
import type { ApiV1DataContentTypesIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ContentTypesApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataContentTypesIdGetRequest;

  try {
    const data = await api.apiV1DataContentTypesIdGet(body);
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

[**ContentTypeDTO**](ContentTypeDTO.md)

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


## apiV1DataContentTypesPost

> apiV1DataContentTypesPost(contentTypeDTO)

Add a single content type to the system.

### Example

```ts
import {
  Configuration,
  ContentTypesApi,
} from '';
import type { ApiV1DataContentTypesPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new ContentTypesApi();

  const body = {
    // ContentTypeDTO |  (optional)
    contentTypeDTO: ...,
  } satisfies ApiV1DataContentTypesPostRequest;

  try {
    const data = await api.apiV1DataContentTypesPost(body);
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
| **contentTypeDTO** | [ContentTypeDTO](ContentTypeDTO.md) |  | [Optional] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/json`
- **Accept**: Not defined


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

