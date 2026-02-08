
# DataStoreSummaryDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`slug` | string
`name` | string
`filesCount` | number
`storageType` | string
`propertiesJson` | string
`projectId` | string
`canWrite` | boolean

## Example

```typescript
import type { DataStoreSummaryDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "slug": null,
  "name": null,
  "filesCount": null,
  "storageType": null,
  "propertiesJson": null,
  "projectId": null,
  "canWrite": null,
} satisfies DataStoreSummaryDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as DataStoreSummaryDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


