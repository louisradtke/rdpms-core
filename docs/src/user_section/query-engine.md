# Metadata Query Engine

The metadata query engine checks whether a metadata JSON document matches a query JSON document.

## What Is Queried

In this project, metadata documents are validated by dedicated JSON Schemas.
For this page, we use:

- metadata schema: `urn:rdpms:core:schema:time-series-container:v1`
- query schema: `urn:rdpms:core:schema:object-query-dsl:v1`

## Example Metadata Document

The following is a simplified `TimeSeriesContainer` metadata example (schema `urn:rdpms:core:schema:time-series-container:v1`):

```json
{
  "topics": [
    { "name": "/odom" },
    { "name": "/image" },
    { "name": "/tf" }
  ]
}
```

## Core Query Ideas

- A query is either:
  - a logical query (`$and`, `$or`, `$not`), or
  - a field query (`"path.to.field": <constraint>`).
- Field paths use dot notation.
- Scalar shorthand is supported:
  - `"topics.0.name": "/odom"` is equivalent to `"topics.0.name": { "$eq": "/odom" }`.

## Operators (With Meaning)

Comparison and scalar operators:

- `$eq`: equal
- `$ne`: not equal
- `$in`: value is in a given list
- `$exists`: path exists (or does not exist)
- `$gt`: greater than
- `$gte`: greater than or equal
- `$lt`: less than
- `$lte`: less than or equal
- `$regex`: regular-expression match on strings

Array operators:

- `$size`: array length equals value
- `$gteSize`: array length is greater than or equal
- `$lteSize`: array length is less than or equal
- `$elemMatch`: at least one array element matches nested query
- `$allElemMatch`: all array elements match nested query
- `$containsAll`: all listed scalar values must be present

## Query Examples

### 1) Container has at least two topics

```json
{
  "topics": { "$gteSize": 2 }
}
```

Meaning: match documents whose `topics` array has length >= 2.

### 2) At least one topic named `/image`

```json
{
  "topics": {
    "$elemMatch": {
      "name": "/image"
    }
  }
}
```

Meaning: there is at least one element in `topics` where `name` equals `/image`.

### 3) Every topic name starts with `/`

```json
{
  "topics": {
    "$allElemMatch": {
      "name": { "$regex": "^/" }
    }
  }
}
```

Meaning: all elements in `topics` satisfy the nested query.
If one topic name does not start with `/`, the document does not match.

### 4) Topic names must include both `/odom` and `/image`

```json
{
  "topics.name": {
    "$containsAll": ["/odom", "/image"]
  }
}
```

Important evaluation detail:

- The path `topics.name` goes through an array of objects.
- The engine flattens all reached `name` values.
- So the effective compared set is like:
  - `["/odom", "/image", "/tf", ...]`
- `$containsAll` checks whether all requested values are in that flattened set.

## Semantics Notes

- `$exists` only checks if a path resolves.
- `$elemMatch` and `$allElemMatch` evaluate nested queries with the current array element as query root.
- Scalar comparisons are strict by type. Example: `"42"` is not equal to `42`.
