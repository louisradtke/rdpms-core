namespace RDPMS.Core.QueryEngine.Parsing;

/// <summary>
/// Raised when a query document cannot be converted to a valid query AST.
/// </summary>
public sealed class QueryParseException : Exception
{
    public QueryParseException(string message)
        : base(message)
    {
    }

    public QueryParseException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
