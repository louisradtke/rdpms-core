
# MetadataQueryDTO


## Properties

Name | Type
------------ | -------------
`mode` | [QueryMode](QueryMode.md)
`queries` | [Array&lt;MetadataQueryPartDTO&gt;](MetadataQueryPartDTO.md)

## Example

```typescript
import type { MetadataQueryDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "mode": null,
  "queries": null,
} satisfies MetadataQueryDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as MetadataQueryDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


