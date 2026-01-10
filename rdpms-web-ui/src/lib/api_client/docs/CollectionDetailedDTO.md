
# CollectionDetailedDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`slug` | string
`name` | string
`description` | string
`dataSetCount` | number
`defaultDataStoreId` | string
`projectId` | string
`metaDateColumns` | [Array&lt;MetaDateCollectionColumnDTO&gt;](MetaDateCollectionColumnDTO.md)

## Example

```typescript
import type { CollectionDetailedDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "slug": null,
  "name": null,
  "description": null,
  "dataSetCount": null,
  "defaultDataStoreId": null,
  "projectId": null,
  "metaDateColumns": null,
} satisfies CollectionDetailedDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as CollectionDetailedDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


