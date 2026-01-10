
# S3FileCreateRequestDTO


## Properties

Name | Type
------------ | -------------
`name` | string
`contentTypeId` | string
`sizeBytes` | number
`plainSHA256Hash` | string
`createdStamp` | Date
`beginStamp` | Date
`endStamp` | Date
`compressionAlgorithm` | string
`compressedSHA256Hash` | string
`compressedSizeBytes` | number

## Example

```typescript
import type { S3FileCreateRequestDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "name": null,
  "contentTypeId": null,
  "sizeBytes": null,
  "plainSHA256Hash": null,
  "createdStamp": null,
  "beginStamp": null,
  "endStamp": null,
  "compressionAlgorithm": null,
  "compressedSHA256Hash": null,
  "compressedSizeBytes": null,
} satisfies S3FileCreateRequestDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as S3FileCreateRequestDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


