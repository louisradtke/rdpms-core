
# ApiV1DataDatasetsGet200ResponseInner

Represents a summary of a dataset, including identifying information, timestamps, state, tags, and metadata fields.

## Properties

Name | Type
------------ | -------------
`kind` | string
`id` | string
`slug` | string
`name` | string
`assignedTags` | [Array&lt;TagDTO&gt;](TagDTO.md)
`createdStampUTC` | Date
`deletedStampUTC` | Date
`beginStampUTC` | Date
`endStampUTC` | Date
`lifecycleState` | string
`deletionState` | [DeletionStateDTO](DeletionStateDTO.md)
`isTimeSeries` | boolean
`metaDates` | [Array&lt;AssignedMetaDateDTO&gt;](AssignedMetaDateDTO.md)
`fileCount` | number
`collectionId` | string
`files` | [Array&lt;FileMetadataSummaryDTO&gt;](FileMetadataSummaryDTO.md)

## Example

```typescript
import type { ApiV1DataDatasetsGet200ResponseInner } from ''

// TODO: Update the object below with actual values
const example = {
  "kind": null,
  "id": null,
  "slug": null,
  "name": null,
  "assignedTags": null,
  "createdStampUTC": null,
  "deletedStampUTC": null,
  "beginStampUTC": null,
  "endStampUTC": null,
  "lifecycleState": null,
  "deletionState": null,
  "isTimeSeries": null,
  "metaDates": null,
  "fileCount": null,
  "collectionId": null,
  "files": null,
} satisfies ApiV1DataDatasetsGet200ResponseInner

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ApiV1DataDatasetsGet200ResponseInner
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


