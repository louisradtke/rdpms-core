
# SealedS3DataSetCreateRequestDTO

Request body for registering an already existing S3-backed dataset and sealing it in one operation. Object keys are relative to the referenced RDPMS.Core.Persistence.Model.S3DataStore.KeyPrefix.

## Properties

Name | Type
------------ | -------------
`slug` | string
`name` | string
`createdStampUTC` | Date
`collectionId` | string
`storeId` | string
`files` | [Array&lt;SealedS3DataSetFileCreateRequestDTO&gt;](SealedS3DataSetFileCreateRequestDTO.md)

## Example

```typescript
import type { SealedS3DataSetCreateRequestDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "slug": null,
  "name": null,
  "createdStampUTC": null,
  "collectionId": null,
  "storeId": null,
  "files": null,
} satisfies SealedS3DataSetCreateRequestDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as SealedS3DataSetCreateRequestDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


