
# AssignedMetaDateDTO


## Properties

Name | Type
------------ | -------------
`metadataKey` | string
`metadataId` | string
`field` | [MetaDateDTO](MetaDateDTO.md)
`inherited` | boolean
`collectionSchemaVerified` | boolean

## Example

```typescript
import type { AssignedMetaDateDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "metadataKey": null,
  "metadataId": null,
  "field": null,
  "inherited": null,
  "collectionSchemaVerified": null,
} satisfies AssignedMetaDateDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as AssignedMetaDateDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


