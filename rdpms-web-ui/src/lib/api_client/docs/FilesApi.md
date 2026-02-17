# FilesApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataFileRefsGet**](FilesApi.md#apiv1datafilerefsget) | **GET** /api/v1/data/file-refs |  |
| [**apiV1DataFilesGet**](FilesApi.md#apiv1datafilesget) | **GET** /api/v1/data/files | Get files. |
| [**apiV1DataFilesIdBlobGet**](FilesApi.md#apiv1datafilesidblobget) | **GET** /api/v1/data/files/{id}/blob | Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned. |
| [**apiV1DataFilesIdContentGet**](FilesApi.md#apiv1datafilesidcontentget) | **GET** /api/v1/data/files/{id}/content | Request redirect to a file. On success, a 302 referring to the final download URL will be returned. |
| [**apiV1DataFilesIdGet**](FilesApi.md#apiv1datafilesidget) | **GET** /api/v1/data/files/{id} | Get details of a single file. |
| [**apiV1DataFilesIdMetadataKeyDelete**](FilesApi.md#apiv1datafilesidmetadatakeydelete) | **DELETE** /api/v1/data/files/{id}/metadata/{key} | Removes metadate relation with resp. key. |
| [**apiV1DataFilesIdMetadataKeyPost**](FilesApi.md#apiv1datafilesidmetadatakeypost) | **POST** /api/v1/data/files/{id}/metadata/{key} | Rename key for metadate relation. |
| [**apiV1DataFilesIdMetadataKeyPut**](FilesApi.md#apiv1datafilesidmetadatakeyput) | **PUT** /api/v1/data/files/{id}/metadata/{key} | Adds or sets meta documents for a file. |



## apiV1DataFileRefsGet

> FileSummaryDTO apiV1DataFileRefsGet(storeGuid, type)



### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFileRefsGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string (optional)
    storeGuid: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string (optional)
    type: type_example,
  } satisfies ApiV1DataFileRefsGetRequest;

  try {
    const data = await api.apiV1DataFileRefsGet(body);
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
| **storeGuid** | `string` |  | [Optional] [Defaults to `undefined`] |
| **type** | `string` |  | [Optional] [Defaults to `undefined`] |

### Return type

[**FileSummaryDTO**](FileSummaryDTO.md)

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


## apiV1DataFilesGet

> Array&lt;FileSummaryDTO&gt; apiV1DataFilesGet(view)

Get files.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // FileListViewMode (optional)
    view: ...,
  } satisfies ApiV1DataFilesGetRequest;

  try {
    const data = await api.apiV1DataFilesGet(body);
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
| **view** | `FileListViewMode` |  | [Optional] [Defaults to `undefined`] [Enum: Summary, Metadata] |

### Return type

[**Array&lt;FileSummaryDTO&gt;**](FileSummaryDTO.md)

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


## apiV1DataFilesIdBlobGet

> Blob apiV1DataFilesIdBlobGet(id)

Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdBlobGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | Id of the file to download.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataFilesIdBlobGetRequest;

  try {
    const data = await api.apiV1DataFilesIdBlobGet(body);
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
| **id** | `string` | Id of the file to download. | [Defaults to `undefined`] |

### Return type

**Blob**

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `application/octet-stream`, `application/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |
| **501** | Not Implemented |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataFilesIdContentGet

> apiV1DataFilesIdContentGet(id)

Request redirect to a file. On success, a 302 referring to the final download URL will be returned.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdContentGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | Id of the file to download.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataFilesIdContentGetRequest;

  try {
    const data = await api.apiV1DataFilesIdContentGet(body);
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
| **id** | `string` | Id of the file to download. | [Defaults to `undefined`] |

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
| **302** | Found |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataFilesIdGet

> FileSummaryDTO apiV1DataFilesIdGet(id)

Get details of a single file.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdGetRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | Id of the file.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
  } satisfies ApiV1DataFilesIdGetRequest;

  try {
    const data = await api.apiV1DataFilesIdGet(body);
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
| **id** | `string` | Id of the file. | [Defaults to `undefined`] |

### Return type

[**FileSummaryDTO**](FileSummaryDTO.md)

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


## apiV1DataFilesIdMetadataKeyDelete

> apiV1DataFilesIdMetadataKeyDelete(id, key)

Removes metadate relation with resp. key.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdMetadataKeyDeleteRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | ID of the file.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | Key to remove the relation for
    key: key_example,
  } satisfies ApiV1DataFilesIdMetadataKeyDeleteRequest;

  try {
    const data = await api.apiV1DataFilesIdMetadataKeyDelete(body);
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
| **id** | `string` | ID of the file. | [Defaults to `undefined`] |
| **key** | `string` | Key to remove the relation for | [Defaults to `undefined`] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `text/plain`, `application/json`, `text/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataFilesIdMetadataKeyPost

> apiV1DataFilesIdMetadataKeyPost(id, key, newKey)

Rename key for metadate relation.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdMetadataKeyPostRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | The file ID.
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | The old key.
    key: key_example,
    // string | The new key. (optional)
    newKey: newKey_example,
  } satisfies ApiV1DataFilesIdMetadataKeyPostRequest;

  try {
    const data = await api.apiV1DataFilesIdMetadataKeyPost(body);
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
| **id** | `string` | The file ID. | [Defaults to `undefined`] |
| **key** | `string` | The old key. | [Defaults to `undefined`] |
| **newKey** | `string` | The new key. | [Optional] [Defaults to `undefined`] |

### Return type

`void` (Empty response body)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: Not defined
- **Accept**: `text/plain`, `application/json`, `text/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **400** | Bad Request |  -  |
| **404** | Not Found |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


## apiV1DataFilesIdMetadataKeyPut

> MetaDateDTO apiV1DataFilesIdMetadataKeyPut(id, key, body)

Adds or sets meta documents for a file.

### Example

```ts
import {
  Configuration,
  FilesApi,
} from '';
import type { ApiV1DataFilesIdMetadataKeyPutRequest } from '';

async function example() {
  console.log("🚀 Testing  SDK...");
  const api = new FilesApi();

  const body = {
    // string | ID of the file
    id: 38400000-8cf0-11bd-b23e-10b96e4ef00d,
    // string | Case-insensitive key of the meta date on the file
    key: key_example,
    // string | JSON meta document (optional)
    body: BYTE_ARRAY_DATA_HERE,
  } satisfies ApiV1DataFilesIdMetadataKeyPutRequest;

  try {
    const data = await api.apiV1DataFilesIdMetadataKeyPut(body);
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
| **id** | `string` | ID of the file | [Defaults to `undefined`] |
| **key** | `string` | Case-insensitive key of the meta date on the file | [Defaults to `undefined`] |
| **body** | `string` | JSON meta document | [Optional] |

### Return type

[**MetaDateDTO**](MetaDateDTO.md)

### Authorization

No authorization required

### HTTP request headers

- **Content-Type**: `application/octet-stream`, `application/json`
- **Accept**: `text/plain`, `application/json`, `text/json`


### HTTP response details
| Status code | Description | Response headers |
|-------------|-------------|------------------|
| **200** | OK |  -  |
| **201** | Created |  -  |
| **400** | Bad Request |  -  |

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)

