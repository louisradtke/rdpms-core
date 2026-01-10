
# ContentTypeDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`abbreviation` | string
`displayName` | string
`description` | string
`mimeType` | string

## Example

```typescript
import type { ContentTypeDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "abbreviation": null,
  "displayName": null,
  "description": null,
  "mimeType": null,
} satisfies ContentTypeDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as ContentTypeDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


