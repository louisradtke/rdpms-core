# DataSetsApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataDatasetsGet**](DataSetsApi.md#apiv1datadatasetsget) | **GET** /api/v1/data/datasets | Get data sets. |
| [**apiV1DataDatasetsIdAddS3Post**](DataSetsApi.md#apiv1datadatasetsidadds3post) | **POST** /api/v1/data/datasets/{id}/add/s3 | Add a single file to the system. Request a single S3 upload URL. |
| [**apiV1DataDatasetsIdDelete**](DataSetsApi.md#apiv1datadatasetsiddelete) | **DELETE** /api/v1/data/datasets/{id} |  |
| [**apiV1DataDatasetsIdGet**](DataSetsApi.md#apiv1datadatasetsidget) | **GET** /api/v1/data/datasets/{id} |  |
| [**apiV1DataDatasetsIdMetadataKeyPut**](DataSetsApi.md#apiv1datadatasetsidmetadatakeyput) | **PUT** /api/v1/data/datasets/{id}/metadata/{key} | Adds or sets meta documents for a data set. |
| [**apiV1DataDatasetsIdSealPut**](DataSetsApi.md#apiv1datadatasetsidsealput) | **PUT** /api/v1/data/datasets/{id}/seal | Seals a data set. Only works for data sets that are in \&quot;Uninitialized\&quot; state. |
| [**apiV1DataDatasetsPost**](DataSetsApi.md#apiv1datadatasetspost) | **POST** /api/v1/data/datasets | Add a single item to the system. |



## apiV1DataDatasetsGet

> Array&lt;DataSetSummaryDTO&gt; apiV1DataDatasetsGet(collectionId, deleted)

Get data sets.

### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string |  (optional)
    collectionId: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | comma-separated list of strings, case-insensitive.             Valid values can be found in RDPMS.Core.Server.Model.DTO.V1.DataSetSummaryDTO (optional)
    deleted: deleted_example,
  } satisfies ApiV1DataDatasetsGetRequest;

  try {
    const data = await api.apiV1DataDatasetsGet(body);
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
| **collectionId** | `string` |  | [Optional] [Defaults to `undefined`] |
| **deleted** | `string` | comma-separated list of strings, case-insensitive.             Valid values can be found in RDPMS.Core.Server.Model.DTO.V1.DataSetSummaryDTO | [Optional] [Defaults to `undefined`] |

### Return type

[**Array&lt;DataSetSummaryDTO&gt;**](DataSetSummaryDTO.md)

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


## apiV1DataDatasetsIdAddS3Post

> FileCreateResponseDTO apiV1DataDatasetsIdAddS3Post(id, s3FileCreateRequestDTO)

Add a single file to the system. Request a single S3 upload URL.

### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsIdAddS3PostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string | 
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // S3FileCreateRequestDTO |  (optional)
    s3FileCreateRequestDTO: ...,
  } satisfies ApiV1DataDatasetsIdAddS3PostRequest;

  try {
    const data = await api.apiV1DataDatasetsIdAddS3Post(body);
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
| **s3FileCreateRequestDTO** | [S3FileCreateRequestDTO](S3FileCreateRequestDTO.md) |  | [Optional] |

### Return type

[**FileCreateResponseDTO**](FileCreateResponseDTO.md)

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
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataDatasetsIdDelete

> apiV1DataDatasetsIdDelete(id)



### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsIdDeleteRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataDatasetsIdDeleteRequest;

  try {
    const data = await api.apiV1DataDatasetsIdDelete(body);
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

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **204** | No Content |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataDatasetsIdGet

> DataSetDetailedDTO apiV1DataDatasetsIdGet(id)



### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataDatasetsIdGetRequest;

  try {
    const data = await api.apiV1DataDatasetsIdGet(body);
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

[**DataSetDetailedDTO**](DataSetDetailedDTO.md)

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


## apiV1DataDatasetsIdMetadataKeyPut

> string apiV1DataDatasetsIdMetadataKeyPut(id, key, body)

Adds or sets meta documents for a data set.

### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsIdMetadataKeyPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string | ID of the data set
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | Case-insensitive key of the meta date on the data set
    key: key_example,
    // string | JSON meta document (optional)
    body: BYTE_ARRAY_DATA_HERE,
  } satisfies ApiV1DataDatasetsIdMetadataKeyPutRequest;

  try {
    const data = await api.apiV1DataDatasetsIdMetadataKeyPut(body);
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
| **id** | `string` | ID of the data set | [Defaults to `undefined`] |
| **key** | `string` | Case-insensitive key of the meta date on the data set | [Defaults to `undefined`] |
| **body** | `string` | JSON meta document | [Optional] |

### Return type

**string**

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/octet-stream`, `application/json`
- **Accept**: `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **201** | Created |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataDatasetsIdSealPut

> apiV1DataDatasetsIdSealPut(id)

Seals a data set. Only works for data sets that are in \&quot;Uninitialized\&quot; state.

### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsIdSealPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // string | The data set id.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataDatasetsIdSealPutRequest;

  try {
    const data = await api.apiV1DataDatasetsIdSealPut(body);
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
| **id** | `string` | The data set id. | [Defaults to `undefined`] |

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
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataDatasetsPost

> string apiV1DataDatasetsPost(dataSetSummaryDTO)

Add a single item to the system.

### Example

```ts
import {
  Configuration,
  DataSetsApi,
} from '';
import type { ApiV1DataDatasetsPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new DataSetsApi();

  const body = {
    // DataSetSummaryDTO |  (optional)
    dataSetSummaryDTO: ...,
  } satisfies ApiV1DataDatasetsPostRequest;

  try {
    const data = await api.apiV1DataDatasetsPost(body);
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
| **dataSetSummaryDTO** | [DataSetSummaryDTO](DataSetSummaryDTO.md) |  | [Optional] |

### Return type

**string**

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

