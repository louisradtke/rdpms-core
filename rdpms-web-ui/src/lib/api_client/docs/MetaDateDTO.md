
# MetaDateDTO


## Properties

Name | Type
------------ | -------------
`id` | string
`validatedSchemas` | [Array&lt;SchemaDTO&gt;](SchemaDTO.md)
`fileId` | string

## Example

```typescript
import type { MetaDateDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "id": null,
  "validatedSchemas": null,
  "fileId": null,
} satisfies MetaDateDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as MetaDateDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


