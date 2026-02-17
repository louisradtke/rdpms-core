
# DataSetCreateRequestDTO

Request body for creating a new dataset. Kept non-polymorphic on purpose to avoid discriminator/subtype binding issues.

## Properties

Name | Type
------------ | -------------
`slug` | string
`name` | string
`createdStampUTC` | Date
`collectionId` | string

## Example

```typescript
import type { DataSetCreateRequestDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "slug": null,
  "name": null,
  "createdStampUTC": null,
  "collectionId": null,
} satisfies DataSetCreateRequestDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as DataSetCreateRequestDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


