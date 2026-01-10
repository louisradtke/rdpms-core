
# ProjectSummaryDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`slug` | string
`name` | string
`description` | string
`collections` | [Array&lt;CollectionSummaryDTO&gt;](CollectionSummaryDTO.md)
`dataStores` | [Array&lt;DataStoreSummaryDTO&gt;](DataStoreSummaryDTO.md)

## Example

```typescript
import type { ProjectSummaryDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "slug": null,
  "name": null,
  "description": null,
  "collections": null,
  "dataStores": null,
} satisfies ProjectSummaryDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ProjectSummaryDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


