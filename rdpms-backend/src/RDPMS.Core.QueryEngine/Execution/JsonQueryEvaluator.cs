using System.Text.Json;
using System.Text.RegularExpressions;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Execution;

/// <summary>
/// In-memory interpreter for the query AST.
/// Supports logical nodes, scalar constraints, and array operators including <c>$elemMatch</c>,
/// <c>$allElemMatch</c>, and <c>$containsAll</c>.
/// </summary>
public sealed class JsonQueryEvaluator : IJsonQueryEvaluator
{
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

    public bool IsMatch(QueryNode query, JsonElement document)
    {
        return EvaluateQuery(query, document);
    }

    private static bool EvaluateQuery(QueryNode query, JsonElement root)
    {
        return query switch
        {
            AndQueryNode andQuery => andQuery.Children.All(child => EvaluateQuery(child, root)),
            OrQueryNode orQuery => orQuery.Children.Any(child => EvaluateQuery(child, root)),
            NotQueryNode notQuery => !EvaluateQuery(notQuery.Child, root),
            FieldQueryNode fieldQuery => fieldQuery.Predicates.All(predicate => EvaluatePredicate(predicate, root)),
            _ => throw new InvalidOperationException($"Unsupported query node type {query.GetType().Name}"),
        };
    }

    private static bool EvaluatePredicate(FieldPredicateNode predicate, JsonElement root)
    {
        if (predicate.Constraint is ContainsAllConstraintNode containsAll)
        {
            return EvaluateContainsAllPredicate(predicate.FieldPath, containsAll, root);
        }

        var exists = TryResolvePath(root, predicate.FieldPath, out var value);

        if (predicate.Constraint is ExistsConstraintNode existsConstraint)
        {
            return exists == existsConstraint.ShouldExist;
        }

        if (!exists)
        {
            return false;
        }

        return EvaluateConstraint(predicate.Constraint, value);
    }

    private static bool EvaluateConstraint(ConstraintNode constraint, JsonElement value)
    {
        return constraint switch
        {
            EqConstraintNode eq => TryReadScalar(value, out var scalar) && scalar.EqualsByDslSemantics(eq.Value),
            NeConstraintNode ne => !TryReadScalar(value, out var scalar) || !scalar.EqualsByDslSemantics(ne.Value),
            InConstraintNode inConstraint => TryReadScalar(value, out var scalar) &&
                                             inConstraint.Values.Any(candidate => scalar.EqualsByDslSemantics(candidate)),
            GtConstraintNode gt => CompareScalars(value, gt.Value, comparison => comparison > 0),
            GteConstraintNode gte => CompareScalars(value, gte.Value, comparison => comparison >= 0),
            LtConstraintNode lt => CompareScalars(value, lt.Value, comparison => comparison < 0),
            LteConstraintNode lte => CompareScalars(value, lte.Value, comparison => comparison <= 0),
            RegexConstraintNode regex => MatchRegex(value, regex.Pattern),
            SizeConstraintNode size => MatchArraySize(value, length => length == size.Length),
            GteSizeConstraintNode gteSize => MatchArraySize(value, length => length >= gteSize.MinimumLength),
            LteSizeConstraintNode lteSize => MatchArraySize(value, length => length <= lteSize.MaximumLength),
            ElemMatchConstraintNode elemMatch => MatchElemMatch(value, elemMatch.ElementQuery),
            AllElemMatchConstraintNode allElemMatch => MatchAllElemMatch(value, allElemMatch.ElementQuery),
            ContainsAllConstraintNode => throw new InvalidOperationException("$containsAll is handled in predicate evaluation."),
            ExistsConstraintNode => throw new InvalidOperationException("$exists is handled before constraint evaluation."),
            _ => throw new InvalidOperationException($"Unsupported constraint type {constraint.GetType().Name}"),
        };
    }

    private static bool EvaluateContainsAllPredicate(string path, ContainsAllConstraintNode containsAll, JsonElement root)
    {
        var resolved = ResolvePathValues(root, path);
        if (resolved.Count == 0)
        {
            return false;
        }

        if (resolved.Count == 1 && resolved[0].ValueKind == JsonValueKind.Array)
        {
            var arrayValues = resolved[0].EnumerateArray()
                .Where(TryReadScalar)
                .Select(ScalarValue.FromJsonElement)
                .ToArray();
            return ContainsAll(arrayValues, containsAll.Values);
        }

        var flattened = resolved.Where(TryReadScalar).Select(ScalarValue.FromJsonElement).ToArray();
        return ContainsAll(flattened, containsAll.Values);
    }

    private static bool ContainsAll(IReadOnlyList<ScalarValue> haystack, IReadOnlyList<ScalarValue> needles)
    {
        foreach (var needle in needles)
        {
            if (!haystack.Any(candidate => candidate.EqualsByDslSemantics(needle)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool CompareScalars(JsonElement documentValue, ScalarValue queryValue, Func<int, bool> predicate)
    {
        if (!TryReadScalar(documentValue, out var documentScalar))
        {
            return false;
        }

        if (documentScalar.Kind != queryValue.Kind)
        {
            return false;
        }

        int comparison = documentScalar.Kind switch
        {
            ScalarValueKind.Number => documentScalar.NumberValue!.Value.CompareTo(queryValue.NumberValue!.Value),
            ScalarValueKind.String => string.Compare(documentScalar.StringValue, queryValue.StringValue, StringComparison.Ordinal),
            _ => int.MinValue,
        };

        if (comparison == int.MinValue)
        {
            return false;
        }

        return predicate(comparison);
    }

    private static bool MatchRegex(JsonElement value, string pattern)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        try
        {
            var source = value.GetString() ?? string.Empty;
            return Regex.IsMatch(source, pattern, RegexOptions.None, RegexTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool MatchArraySize(JsonElement value, Func<int, bool> predicate)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        return predicate(value.GetArrayLength());
    }

    private static bool MatchElemMatch(JsonElement value, QueryNode elementQuery)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in value.EnumerateArray())
        {
            if (EvaluateQuery(elementQuery, item))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchAllElemMatch(JsonElement value, QueryNode elementQuery)
    {
        if (value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (var item in value.EnumerateArray())
        {
            if (!EvaluateQuery(elementQuery, item))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryResolvePath(JsonElement root, string path, out JsonElement value)
    {
        value = root;

        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (value.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            if (!value.TryGetProperty(segment, out var child))
            {
                value = default;
                return false;
            }

            value = child;
        }

        return true;
    }

    private static List<JsonElement> ResolvePathValues(JsonElement root, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return [];
        }

        var current = new List<JsonElement> { root };

        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var next = new List<JsonElement>();
            foreach (var node in current)
            {
                if (node.ValueKind == JsonValueKind.Object)
                {
                    if (node.TryGetProperty(segment, out var child))
                    {
                        next.Add(child);
                    }

                    continue;
                }

                if (node.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in node.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(segment, out var child))
                        {
                            next.Add(child);
                        }
                    }
                }
            }

            current = next;
            if (current.Count == 0)
            {
                return [];
            }
        }

        return current;
    }

    private static bool TryReadScalar(JsonElement value, out ScalarValue scalar)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                scalar = ScalarValue.FromJsonElement(value);
                return true;
            default:
                scalar = default;
                return false;
        }
    }

    private static bool TryReadScalar(JsonElement value)
    {
        return TryReadScalar(value, out _);
    }
}
