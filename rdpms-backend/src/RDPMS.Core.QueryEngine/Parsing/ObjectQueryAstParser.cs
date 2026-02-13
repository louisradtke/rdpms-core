using System.Text.Json;
using Corvus.Json;
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
        var root = new ObjectQueryDslV1Schema(queryJson);
        return Parse(root);
    }

    public QueryNode Parse(ObjectQueryDslV1Schema query)
    {
        if (!query.IsValid())
        {
            throw new QueryParseException("Query does not match ObjectQueryDslV1 schema.");
        }

        var rootQuery = query.AsAllOf0Entity;
        if (rootQuery.TryGetAsLogicalQuery(out var logicalQuery))
        {
            return ParseLogicalQuery(logicalQuery);
        }

        if (rootQuery.TryGetAsFieldQuery(out var fieldQuery))
        {
            return ParseFieldQuery(fieldQuery);
        }

        throw new QueryParseException("Root query is neither logical nor field query.");
    }

    private static QueryNode ParseQuery(ObjectQueryDslV1Schema.Query query)
    {
        if (query.TryGetAsLogicalQuery(out var logicalQuery))
        {
            return ParseLogicalQuery(logicalQuery);
        }

        if (query.TryGetAsFieldQuery(out var fieldQuery))
        {
            return ParseFieldQuery(fieldQuery);
        }

        throw new QueryParseException("Query is neither logical nor field query.");
    }

    private static QueryNode ParseElementQuery(ObjectQueryDslV1Schema.ElementQuery query)
    {
        if (query.TryGetAsLogicalElementQuery(out var logicalQuery))
        {
            return ParseLogicalElementQuery(logicalQuery);
        }

        if (query.TryGetAsFieldElementQuery(out var fieldQuery))
        {
            return ParseFieldElementQuery(fieldQuery);
        }

        throw new QueryParseException("Element query is neither logical nor field query.");
    }

    private static QueryNode ParseLogicalQuery(ObjectQueryDslV1Schema.LogicalQuery logical)
    {
        if (logical.And is { } and)
        {
            return new AndQueryNode(and.Select(ParseQuery).ToArray());
        }

        if (logical.Or is { } or)
        {
            return new OrQueryNode(or.Select(ParseQuery).ToArray());
        }

        if (logical.Not is { } not)
        {
            return new NotQueryNode(ParseQuery(not));
        }

        throw new QueryParseException("Logical query has no supported operator.");
    }

    private static QueryNode ParseLogicalElementQuery(ObjectQueryDslV1Schema.LogicalElementQuery logical)
    {
        if (logical.And is { } and)
        {
            return new AndQueryNode(and.Select(ParseElementQuery).ToArray());
        }

        if (logical.Or is { } or)
        {
            return new OrQueryNode(or.Select(ParseElementQuery).ToArray());
        }

        if (logical.Not is { } not)
        {
            return new NotQueryNode(ParseElementQuery(not));
        }

        throw new QueryParseException("Logical element query has no supported operator.");
    }

    private static QueryNode ParseFieldQuery(ObjectQueryDslV1Schema.FieldQuery fieldQuery)
    {
        var predicates = new List<FieldPredicateNode>();
        foreach (var property in fieldQuery.EnumerateObject())
        {
            var path = property.Name.GetString() ?? string.Empty;
            var fieldConstraint = property.Value.As<ObjectQueryDslV1Schema.FieldConstraint>();
            predicates.Add(new FieldPredicateNode(path, ParseFieldConstraint(fieldConstraint)));
        }

        return new FieldQueryNode(predicates);
    }

    private static QueryNode ParseFieldElementQuery(ObjectQueryDslV1Schema.FieldElementQuery fieldQuery)
    {
        var predicates = new List<FieldPredicateNode>();
        foreach (var property in fieldQuery.EnumerateObject())
        {
            var path = property.Name.GetString() ?? string.Empty;
            var fieldConstraint = property.Value.As<ObjectQueryDslV1Schema.FieldConstraint>();
            predicates.Add(new FieldPredicateNode(path, ParseFieldConstraint(fieldConstraint)));
        }

        return new FieldQueryNode(predicates);
    }

    private static ConstraintNode ParseFieldConstraint(ObjectQueryDslV1Schema.FieldConstraint constraint)
    {
        if (constraint.TryGetAsScalar(out var scalar))
        {
            return new EqConstraintNode(ParseScalar(scalar));
        }

        if (constraint.TryGetAsConstraintObject(out var constraintObject))
        {
            return ParseConstraintObject(constraintObject);
        }

        throw new QueryParseException("Field constraint is neither scalar nor object.");
    }

    private static ConstraintNode ParseConstraintObject(ObjectQueryDslV1Schema.ConstraintObject constraint)
    {
        if (constraint.Eq is { } eq) return new EqConstraintNode(ParseScalar(eq));
        if (constraint.Ne is { } ne) return new NeConstraintNode(ParseScalar(ne));

        if (constraint.In is { } inValues)
        {
            var values = inValues.Select(ParseScalar).ToArray();
            return new InConstraintNode(values);
        }

        if (constraint.Exists is { } exists)
        {
            return new ExistsConstraintNode(exists.AsJsonElement.GetBoolean());
        }

        if (constraint.Gt is { } gt) return new GtConstraintNode(ParseScalar(gt));
        if (constraint.Gte is { } gte) return new GteConstraintNode(ParseScalar(gte));
        if (constraint.Lt is { } lt) return new LtConstraintNode(ParseScalar(lt));
        if (constraint.Lte is { } lte) return new LteConstraintNode(ParseScalar(lte));

        if (constraint.Regex is { } regex)
        {
            return new RegexConstraintNode(regex.AsJsonElement.GetString() ?? string.Empty);
        }

        if (constraint.Size is { } size)
        {
            return new SizeConstraintNode(size.AsJsonElement.GetInt32());
        }

        if (constraint.GteSize is { } gteSize)
        {
            return new GteSizeConstraintNode(gteSize.AsJsonElement.GetInt32());
        }

        if (constraint.LteSize is { } lteSize)
        {
            return new LteSizeConstraintNode(lteSize.AsJsonElement.GetInt32());
        }

        if (constraint.ElemMatch is { } elemMatch)
        {
            return new ElemMatchConstraintNode(ParseElementQuery(elemMatch));
        }

        throw new QueryParseException("Constraint object has no supported operator.");
    }

    private static ScalarValue ParseScalar(ObjectQueryDslV1Schema.Scalar scalar)
    {
        return ScalarValue.FromJsonElement(scalar.AsJsonElement);
    }
}
