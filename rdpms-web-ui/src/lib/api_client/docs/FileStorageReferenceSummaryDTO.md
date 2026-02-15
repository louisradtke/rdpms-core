
# FileStorageReferenceSummaryDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`compressionAlgorithm` | string
`sizeBytes` | number
`shA256Hash` | string
`storageType` | string

## Example

```typescript
import type { FileStorageReferenceSummaryDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "compressionAlgorithm": null,
  "sizeBytes": null,
  "shA256Hash": null,
  "storageType": null,
} satisfies FileStorageReferenceSummaryDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as FileStorageReferenceSummaryDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


