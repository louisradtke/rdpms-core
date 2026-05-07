
# ExecutionFinishedRequestDTO


## Properties

Name | Type
------------ | -------------
`name` | string
`pipelineKey` | string
`trackerId` | string
`sourceDatasetIds` | Array&lt;string&gt;
`outputDatasetIds` | Array&lt;string&gt;
`metadataIds` | Array&lt;string&gt;
`outputCollectionId` | string
`startedStampUtc` | Date
`terminatedStampUtc` | Date

## Example

```typescript
import type { ExecutionFinishedRequestDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "name": null,
  "pipelineKey": null,
  "trackerId": null,
  "sourceDatasetIds": null,
  "outputDatasetIds": null,
  "metadataIds": null,
  "outputCollectionId": null,
  "startedStampUtc": null,
  "terminatedStampUtc": null,
} satisfies ExecutionFinishedRequestDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ExecutionFinishedRequestDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


