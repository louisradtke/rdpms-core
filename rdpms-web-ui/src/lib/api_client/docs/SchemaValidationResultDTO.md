
# SchemaValidationResultDTO


## Properties

Name | Type
------------ | -------------
`succesful` | boolean
`reasons` | Array&lt;string&gt;
`traces` | Array&lt;string&gt;

## Example

```typescript
import type { SchemaValidationResultDTO } from ''

// TODO: Update the object below with actual values
const example = {
  "succesful": null,
  "reasons": null,
  "traces": null,
} satisfies SchemaValidationResultDTO

console.log(example)

// Convert the instance to a JSON string
const exampleJSON: string = JSON.stringify(example)
console.log(exampleJSON)

// Parse the JSON string back to an object
const exampleParsed = JSON.parse(exampleJSON) as SchemaValidationResultDTO
console.log(exampleParsed)
```

[[Back to top]](#) [[Back to API list]](../README.md#api-endpoints) [[Back to Model list]](../README.md#models) [[Back to README]](../README.md)


