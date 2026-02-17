# CollectionsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataCollectionsGet**](CollectionsApi.md#apiv1datacollectionsget) | **GET** /api/v1/data/collections | Get all collections. |
| [**apiV1DataCollectionsIdGet**](CollectionsApi.md#apiv1datacollectionsidget) | **GET** /api/v1/data/collections/{id} | Get a single collection by id. |
| [**apiV1DataCollectionsIdMetadataKeyDelete**](CollectionsApi.md#apiv1datacollectionsidmetadatakeydelete) | **DELETE** /api/v1/data/collections/{id}/metadata/{key} |  |
| [**apiV1DataCollectionsIdMetadataKeyPost**](CollectionsApi.md#apiv1datacollectionsidmetadatakeypost) | **POST** /api/v1/data/collections/{id}/metadata/{key} |  |
| [**apiV1DataCollectionsIdMetadataKeyPut**](CollectionsApi.md#apiv1datacollectionsidmetadatakeyput) | **PUT** /api/v1/data/collections/{id}/metadata/{key} |  |
| [**apiV1DataCollectionsPost**](CollectionsApi.md#apiv1datacollectionspost) | **POST** /api/v1/data/collections | Add a single item to the system. |



## apiV1DataCollectionsGet

> Array&lt;CollectionSummaryDTO&gt; apiV1DataCollectionsGet(projectId, projectSlug, slug)

Get all collections.

### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // string | Used to filter for collections with this parent project (optional)
    projectId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | Used to filter for collections with this parent project (optional)
    projectSlug: projectSlug_example,
    // string | Used to filter for collections with this slug (optional)
    slug: slug_example,
  } satisfies ApiV1DataCollectionsGetRequest;

  try {
    const data = await api.apiV1DataCollectionsGet(body);
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
| **projectId** | `string` | Used to filter for collections with this parent project | [Optional] [Defaults to `undefined`] |
| **projectSlug** | `string` | Used to filter for collections with this parent project | [Optional] [Defaults to `undefined`] |
| **slug** | `string` | Used to filter for collections with this slug | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;CollectionSummaryDTO&gt;**](CollectionSummaryDTO.md)

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


## apiV1DataCollectionsIdGet

> CollectionDetailedDTO apiV1DataCollectionsIdGet(id)

Get a single collection by id.

### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataCollectionsIdGetRequest;

  try {
    const data = await api.apiV1DataCollectionsIdGet(body);
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

[**CollectionDetailedDTO**](CollectionDetailedDTO.md)

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


## apiV1DataCollectionsIdMetadataKeyDelete

> apiV1DataCollectionsIdMetadataKeyDelete(id, key, target)



### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsIdMetadataKeyDeleteRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    key: key_example,
    // MetadataColumnTargetDTO (optional)
    target: ...,
  } satisfies ApiV1DataCollectionsIdMetadataKeyDeleteRequest;

  try {
    const data = await api.apiV1DataCollectionsIdMetadataKeyDelete(body);
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
| **key** | `string` |  | [Defaults to `undefined`] |
| **target** | `MetadataColumnTargetDTO` |  | [Optional] [Defaults to `undefined`] [Enum: Dataset, File] |

### Return type

`void` (Empty response body)

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


## apiV1DataCollectionsIdMetadataKeyPost

> apiV1DataCollectionsIdMetadataKeyPost(id, key, newKey, target)



### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsIdMetadataKeyPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    key: key_example,
    // string (optional)
    newKey: newKey_example,
    // MetadataColumnTargetDTO (optional)
    target: ...,
  } satisfies ApiV1DataCollectionsIdMetadataKeyPostRequest;

  try {
    const data = await api.apiV1DataCollectionsIdMetadataKeyPost(body);
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
| **key** | `string` |  | [Defaults to `undefined`] |
| **newKey** | `string` |  | [Optional] [Defaults to `undefined`] |
| **target** | `MetadataColumnTargetDTO` |  | [Optional] [Defaults to `undefined`] [Enum: Dataset, File] |

### Return type

`void` (Empty response body)

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


## apiV1DataCollectionsIdMetadataKeyPut

> apiV1DataCollectionsIdMetadataKeyPut(id, key, schemaId, defaultMetadataId, target)



### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsIdMetadataKeyPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string
    key: key_example,
    // string (optional)
    schemaId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    defaultMetadataId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // MetadataColumnTargetDTO (optional)
    target: ...,
  } satisfies ApiV1DataCollectionsIdMetadataKeyPutRequest;

  try {
    const data = await api.apiV1DataCollectionsIdMetadataKeyPut(body);
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
| **key** | `string` |  | [Defaults to `undefined`] |
| **schemaId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **defaultMetadataId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **target** | `MetadataColumnTargetDTO` |  | [Optional] [Defaults to `undefined`] [Enum: Dataset, File] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **201** | Created |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataCollectionsPost

> apiV1DataCollectionsPost(collectionSummaryDTO)

Add a single item to the system.

### Example

```ts
import {
  Configuration,
  CollectionsApi,
} from '';
import type { ApiV1DataCollectionsPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new CollectionsApi();

  const body = {
    // CollectionSummaryDTO |  (optional)
    collectionSummaryDTO: ...,
  } satisfies ApiV1DataCollectionsPostRequest;

  try {
    const data = await api.apiV1DataCollectionsPost(body);
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
| **collectionSummaryDTO** | [CollectionSummaryDTO](CollectionSummaryDTO.md) |  | [Optional] |

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
| **200** | OK |  -  |
| **404** | Not Found |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

