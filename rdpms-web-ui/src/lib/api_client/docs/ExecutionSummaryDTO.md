
# ExecutionSummaryDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`name` | string
`pipelineKey` | string
`trackerId` | string
`state` | [JobState](JobState.md)
`createdStampUtc` | Date
`startedStampUtc` | Date
`terminatedStampUtc` | Date
`sourceDatasetIds` | Array&lt;string&gt;
`outputDatasetIds` | Array&lt;string&gt;
`metadataIds` | Array&lt;string&gt;

## Example

```typescript
import type { ExecutionSummaryDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "name": null,
  "pipelineKey": null,
  "trackerId": null,
  "state": null,
  "createdStampUtc": null,
  "startedStampUtc": null,
  "terminatedStampUtc": null,
  "sourceDatasetIds": null,
  "outputDatasetIds": null,
  "metadataIds": null,
} satisfies ExecutionSummaryDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ExecutionSummaryDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


