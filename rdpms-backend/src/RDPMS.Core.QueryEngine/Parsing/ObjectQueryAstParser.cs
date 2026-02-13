using System.Text.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Parsing;

public sealed class ObjectQueryAstParser : IObjectQueryAstParser
{
    public QueryNode Parse(string queryJson)
    {
        try
        {
            using var document = JsonDocument.Parse(queryJson);
            return Parse(document.RootElement);
        }
        catch (JsonException exception)
        {
            throw new QueryParseException("Could not parse query JSON.", exception);
        }
    }

    public QueryNode Parse(JsonElement queryJson)
    {
        return ParseQueryJson(queryJson, isElementContext: false, allowSchemaProperty: true);
    }

    public QueryNode Parse(ObjectQueryDslV1Schema query)
    {
        return Parse(query.AsJsonElement);
    }

    private static QueryNode ParseQueryJson(JsonElement queryJson, bool isElementContext, bool allowSchemaProperty)
    {
        if (queryJson.ValueKind != JsonValueKind.Object)
        {
            throw new QueryParseException("Query node must be a JSON object.");
        }

        var logicalKeys = new List<string>();
        if (queryJson.TryGetProperty("$and", out _)) logicalKeys.Add("$and");
        if (queryJson.TryGetProperty("$or", out _)) logicalKeys.Add("$or");
        if (queryJson.TryGetProperty("$not", out _)) logicalKeys.Add("$not");

        if (logicalKeys.Count > 1)
        {
            throw new QueryParseException("Query node may only contain one of $and, $or, or $not.");
        }

        if (logicalKeys.Count == 1)
        {
            var key = logicalKeys[0];
            if (key == "$and")
            {
                var andArray = queryJson.GetProperty("$and");
                if (andArray.ValueKind != JsonValueKind.Array)
                {
                    throw new QueryParseException("$and must be an array.");
                }

                var children = andArray.EnumerateArray()
                    .Select(item => ParseQueryJson(item, isElementContext, allowSchemaProperty: false))
                    .ToArray();
                if (children.Length == 0)
                {
                    throw new QueryParseException("$and array must not be empty.");
                }

                return new AndQueryNode(children);
            }

            if (key == "$or")
            {
                var orArray = queryJson.GetProperty("$or");
                if (orArray.ValueKind != JsonValueKind.Array)
                {
                    throw new QueryParseException("$or must be an array.");
                }

                var children = orArray.EnumerateArray()
                    .Select(item => ParseQueryJson(item, isElementContext, allowSchemaProperty: false))
                    .ToArray();
                if (children.Length == 0)
                {
                    throw new QueryParseException("$or array must not be empty.");
                }

                return new OrQueryNode(children);
            }

            var notQuery = queryJson.GetProperty("$not");
            return new NotQueryNode(ParseQueryJson(notQuery, isElementContext, allowSchemaProperty: false));
        }

        var predicates = new List<FieldPredicateNode>();
        foreach (var property in queryJson.EnumerateObject())
        {
            if (allowSchemaProperty && property.NameEquals("$schema"))
            {
                continue;
            }

            if (property.Name.StartsWith('$'))
            {
                throw new QueryParseException($"Unsupported operator '{property.Name}' in query node.");
            }

            var constraint = ParseFieldConstraintJson(property.Value, isElementContext);
            predicates.Add(new FieldPredicateNode(property.Name, constraint));
        }

        if (predicates.Count == 0)
        {
            throw new QueryParseException("Field query must contain at least one field constraint.");
        }

        return new FieldQueryNode(predicates);
    }

    private static ConstraintNode ParseFieldConstraintJson(JsonElement constraintJson, bool isElementContext)
    {
        if (IsScalar(constraintJson))
        {
            return new EqConstraintNode(ScalarValue.FromJsonElement(constraintJson));
        }

        if (constraintJson.ValueKind != JsonValueKind.Object)
        {
            throw new QueryParseException("Field constraint must be either a scalar or an object.");
        }

        var operators = new[]
        {
            "$eq", "$ne", "$in", "$exists", "$gt", "$gte", "$lt", "$lte", "$regex", "$size", "$gteSize",
            "$lteSize", "$elemMatch", "$allElemMatch", "$containsAll"
        };

        var presentOperators = operators.Where(op => constraintJson.TryGetProperty(op, out _)).ToArray();
        if (presentOperators.Length != 1)
        {
            throw new QueryParseException("Constraint object must contain exactly one supported operator.");
        }

        var op = presentOperators[0];
        var value = constraintJson.GetProperty(op);

        return op switch
        {
            "$eq" => new EqConstraintNode(ParseScalar(value)),
            "$ne" => new NeConstraintNode(ParseScalar(value)),
            "$in" => new InConstraintNode(ParseScalarArray(value, "$in")),
            "$exists" => ParseExists(value),
            "$gt" => new GtConstraintNode(ParseScalar(value)),
            "$gte" => new GteConstraintNode(ParseScalar(value)),
            "$lt" => new LtConstraintNode(ParseScalar(value)),
            "$lte" => new LteConstraintNode(ParseScalar(value)),
            "$regex" => ParseRegex(value),
            "$size" => new SizeConstraintNode(ParseNonNegativeInt(value, "$size")),
            "$gteSize" => new GteSizeConstraintNode(ParseNonNegativeInt(value, "$gteSize")),
            "$lteSize" => new LteSizeConstraintNode(ParseNonNegativeInt(value, "$lteSize")),
            "$elemMatch" => new ElemMatchConstraintNode(ParseQueryJson(value, isElementContext: true, allowSchemaProperty: false)),
            "$allElemMatch" => new AllElemMatchConstraintNode(ParseQueryJson(value, isElementContext: true, allowSchemaProperty: false)),
            "$containsAll" => new ContainsAllConstraintNode(ParseScalarArray(value, "$containsAll")),
            _ => throw new QueryParseException($"Unsupported operator '{op}'."),
        };
    }

    private static ScalarValue ParseScalar(JsonElement value)
    {
        if (!IsScalar(value))
        {
            throw new QueryParseException("Expected scalar value.");
        }

        return ScalarValue.FromJsonElement(value);
    }

    private static IReadOnlyList<ScalarValue> ParseScalarArray(JsonElement value, string operatorName)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            throw new QueryParseException($"{operatorName} must be an array.");
        }

        var result = new List<ScalarValue>();
        foreach (var item in value.EnumerateArray())
        {
            if (!IsScalar(item))
            {
                throw new QueryParseException($"{operatorName} array must only contain scalar values.");
            }

            result.Add(ScalarValue.FromJsonElement(item));
        }

        if (result.Count == 0)
        {
            throw new QueryParseException($"{operatorName} array must not be empty.");
        }

        return result;
    }

    private static ExistsConstraintNode ParseExists(JsonElement value)
    {
        if (value.ValueKind is JsonValueKind.True)
        {
            return new ExistsConstraintNode(true);
        }

        if (value.ValueKind is JsonValueKind.False)
        {
            return new ExistsConstraintNode(false);
        }

        throw new QueryParseException("$exists must be a boolean.");
    }

    private static RegexConstraintNode ParseRegex(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            throw new QueryParseException("$regex must be a string.");
        }

        return new RegexConstraintNode(value.GetString() ?? string.Empty);
    }

    private static int ParseNonNegativeInt(JsonElement value, string operatorName)
    {
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out var intValue) || intValue < 0)
        {
            throw new QueryParseException($"{operatorName} must be a non-negative integer.");
        }

        return intValue;
    }

    private static bool IsScalar(JsonElement value)
    {
        return value.ValueKind is JsonValueKind.String
            or JsonValueKind.Number
            or JsonValueKind.True
            or JsonValueKind.False
            or JsonValueKind.Null;
    }
}
