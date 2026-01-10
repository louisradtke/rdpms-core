# FilesApi

All URIs are relative to *http://localhost*

| Method | HTTP request | Description |
|------------- | ------------- | -------------|
| [**apiV1DataFileRefsGet**](FilesApi.md#apiv1datafilerefsget) | **GET** /api/v1/data/file-refs |  |
| [**apiV1DataFilesGet**](FilesApi.md#apiv1datafilesget) | **GET** /api/v1/data/files | Get summaries of all files. |
| [**apiV1DataFilesIdBlobGet**](FilesApi.md#apiv1datafilesidblobget) | **GET** /api/v1/data/files/{id}/blob | Request a download of a file. Not all files are available for download. On success, the raw bytes will be returned. |
| [**apiV1DataFilesIdContentGet**](FilesApi.md#apiv1datafilesidcontentget) | **GET** /api/v1/data/files/{id}/content | Request redirect to a file. On success, a 302 referring to the final download URL will be returned. |
| [**apiV1DataFilesIdGet**](FilesApi.md#apiv1datafilesidget) | **GET** /api/v1/data/files/{id} | Get summary of a single file. |



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

> Array&lt;FileSummaryDTO&gt; apiV1DataFilesGet()

Get summaries of all files.

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

  try {
    const data = await api.apiV1DataFilesGet();
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

Get summary of a single file.

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

