
# DataSetDetailedDTOAllOfFiles


## Properties

Name | Type
------------ | -------------
`kind` | string
`id` | string
`name` | string
`downloadURI` | string
`contentType` | [ContentTypeDTO](ContentTypeDTO.md)
`size` | number
`createdStampUTC` | Date
`deletedStampUTC` | Date
`beginStampUTC` | Date
`endStampUTC` | Date
`isTimeSeries` | boolean
`metaDates` | [Array&lt;AssignedMetaDateDTO&gt;](AssignedMetaDateDTO.md)
`deletionState` | [DeletionStateDTO](DeletionStateDTO.md)
`references` | [Array&lt;FileStorageReferenceSummaryDTO&gt;](FileStorageReferenceSummaryDTO.md)

## Example

```typescript
import type { DataSetDetailedDTOAllOfFiles } from ''

// TODO: Update the object below with actual values
const example = {
  "kind": null,
  "id": null,
  "name": null,
  "downloadURI": null,
  "contentType": null,
  "size": null,
  "createdStampUTC": null,
  "deletedStampUTC": null,
  "beginStampUTC": null,
  "endStampUTC": null,
  "isTimeSeries": null,
  "metaDates": null,
  "deletionState": null,
  "references": null,
} satisfies DataSetDetailedDTOAllOfFiles

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as DataSetDetailedDTOAllOfFiles
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


