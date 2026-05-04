
# SealedS3DataSetFileCreateRequestDTO

File entry for RDPMS.Core.Server.Model.DTO.V1.SealedS3DataSetCreateRequestDTO.

## Properties

Name | Type
------------ | -------------
`name` | string
`objectKey` | string
`contentTypeId` | string
`sizeBytes` | number
`plainSHA256Hash` | string
`storedSHA256Hash` | string
`compressionAlgorithm` | string
`createdStampUTC` | Date
`beginStampUTC` | Date
`endStampUTC` | Date

## Example

```typescript
import type { SealedS3DataSetFileCreateRequestDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "name": null,
  "objectKey": null,
  "contentTypeId": null,
  "sizeBytes": null,
  "plainSHA256Hash": null,
  "storedSHA256Hash": null,
  "compressionAlgorithm": null,
  "createdStampUTC": null,
  "beginStampUTC": null,
  "endStampUTC": null,
} satisfies SealedS3DataSetFileCreateRequestDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as SealedS3DataSetFileCreateRequestDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


