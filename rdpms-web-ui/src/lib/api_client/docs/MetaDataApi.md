# MetaDataApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataMetadataGet**](MetaDataApi.md#apiv1datametadataget) | **GET** /api/v1/data/metadata | Get all metadata. Contents can be retrieved via Files API. |
| [**apiV1DataMetadataIdGet**](MetaDataApi.md#apiv1datametadataidget) | **GET** /api/v1/data/metadata/{id} | Get a single meta date by id. Content can be retrieved via Files API. |
| [**apiV1DataMetadataIdValidateSchemaIdPut**](MetaDataApi.md#apiv1datametadataidvalidateschemaidput) | **PUT** /api/v1/data/metadata/{id}/validate/{schemaId} | Validate meta date against a schema. On success, the meta date gets updated and true gets returned. If meta date cannot be validated, false gets returned. |
| [**apiV1DataSchemasGet**](MetaDataApi.md#apiv1dataschemasget) | **GET** /api/v1/data/schemas | Get all schemas registered in the system. |
| [**apiV1DataSchemasIdBlobGet**](MetaDataApi.md#apiv1dataschemasidblobget) | **GET** /api/v1/data/schemas/{id}/blob | Get the raw value of a schema. |
| [**apiV1DataSchemasPost**](MetaDataApi.md#apiv1dataschemaspost) | **POST** /api/v1/data/schemas | Add a new schema to the system. |



## apiV1DataMetadataGet

> Array&lt;MetaDateDTO&gt; apiV1DataMetadataGet()

Get all metadata. Contents can be retrieved via Files API.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataMetadataGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  try {
    const data = await api.apiV1DataMetadataGet();
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

[**Array&lt;MetaDateDTO&gt;**](MetaDateDTO.md)

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


## apiV1DataMetadataIdGet

> MetaDateDTO apiV1DataMetadataIdGet(id)

Get a single meta date by id. Content can be retrieved via Files API.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataMetadataIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  const body = {
    // string | Id of the meta date
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataMetadataIdGetRequest;

  try {
    const data = await api.apiV1DataMetadataIdGet(body);
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
| **id** | `string` | Id of the meta date | [Defaults to `undefined`] |

### Return type

[**MetaDateDTO**](MetaDateDTO.md)

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


## apiV1DataMetadataIdValidateSchemaIdPut

> boolean apiV1DataMetadataIdValidateSchemaIdPut(id, schemaId)

Validate meta date against a schema. On success, the meta date gets updated and true gets returned. If meta date cannot be validated, false gets returned.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataMetadataIdValidateSchemaIdPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  const body = {
    // string | ID
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | 
    schemaId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataMetadataIdValidateSchemaIdPutRequest;

  try {
    const data = await api.apiV1DataMetadataIdValidateSchemaIdPut(body);
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
| **id** | `string` | ID | [Defaults to `undefined`] |
| **schemaId** | `string` |  | [Defaults to `undefined`] |

### Return type

**boolean**

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


## apiV1DataSchemasGet

> Array&lt;SchemaDTO&gt; apiV1DataSchemasGet()

Get all schemas registered in the system.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataSchemasGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  try {
    const data = await api.apiV1DataSchemasGet();
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

[**Array&lt;SchemaDTO&gt;**](SchemaDTO.md)

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


## apiV1DataSchemasIdBlobGet

> Blob apiV1DataSchemasIdBlobGet(id)

Get the raw value of a schema.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataSchemasIdBlobGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  const body = {
    // string | Guid of the schema.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataSchemasIdBlobGetRequest;

  try {
    const data = await api.apiV1DataSchemasIdBlobGet(body);
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
| **id** | `string` | Guid of the schema. | [Defaults to `undefined`] |

### Return type

**Blob**

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


## apiV1DataSchemasPost

> apiV1DataSchemasPost(body)

Add a new schema to the system.

### Example

```ts
import {
  Configuration,
  MetaDataApi,
} from '';
import type { ApiV1DataSchemasPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new MetaDataApi();

  const body = {
    // any | The JSON document (optional)
    body: ...,
  } satisfies ApiV1DataSchemasPostRequest;

  try {
    const data = await api.apiV1DataSchemasPost(body);
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
| **body** | `any` | The JSON document | [Optional] |

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
| **201** | Created |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

