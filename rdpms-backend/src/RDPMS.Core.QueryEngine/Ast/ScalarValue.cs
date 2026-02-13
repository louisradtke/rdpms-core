using System.Globalization;
using System.Text.Json;

namespace RDPMS.Core.QueryEngine.Ast;

/// <summary>
/// Normalized scalar categories supported by the query DSL.
/// </summary>
public enum ScalarValueKind
{
    String,
    Number,
    Boolean,
    Null,
}

/// <summary>
/// Type-safe scalar wrapper used by AST constraints and evaluator comparisons.
/// </summary>
public readonly record struct ScalarValue(ScalarValueKind Kind, string? StringValue, double? NumberValue, bool? BoolValue)
{
    public static ScalarValue FromString(string value) => new(ScalarValueKind.String, value, null, null);

    public static ScalarValue FromNumber(double value) => new(ScalarValueKind.Number, null, value, null);

    public static ScalarValue FromBoolean(bool value) => new(ScalarValueKind.Boolean, null, null, value);

    public static ScalarValue Null() => new(ScalarValueKind.Null, null, null, null);

    public static ScalarValue FromJsonElement(JsonElement value)
    {
        return value.ValueKind switch
        {
            JsonValueKind.String => FromString(value.GetString() ?? string.Empty),
            JsonValueKind.Number => FromNumber(value.GetDouble()),
            JsonValueKind.True => FromBoolean(true),
            JsonValueKind.False => FromBoolean(false),
            JsonValueKind.Null => Null(),
            _ => throw new InvalidOperationException($"Value kind {value.ValueKind} is not a scalar."),
        };
    }

    public bool EqualsByDslSemantics(ScalarValue other)
    {
        if (Kind != other.Kind)
        {
            return false;
        }

        return Kind switch
        {
            ScalarValueKind.String => string.Equals(StringValue, other.StringValue, StringComparison.Ordinal),
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            ScalarValueKind.Number => NumberValue == other.NumberValue,
            ScalarValueKind.Boolean => BoolValue == other.BoolValue,
            ScalarValueKind.Null => true,
            _ => false,
        };
    }

    public override string ToString()
    {
        return Kind switch
        {
            ScalarValueKind.String => StringValue ?? string.Empty,
            ScalarValueKind.Number => NumberValue?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            ScalarValueKind.Boolean => BoolValue?.ToString() ?? string.Empty,
            ScalarValueKind.Null => "null",
            _ => string.Empty,
        };
    }
}
