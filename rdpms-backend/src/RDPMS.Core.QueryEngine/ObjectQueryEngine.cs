using System.Text.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine.Ast;
using RDPMS.Core.QueryEngine.Execution;
using RDPMS.Core.QueryEngine.Parsing;

namespace RDPMS.Core.QueryEngine;

/// <summary>
/// Facade for query parsing and execution.
/// Intended as the entry point for callers that need to parse DSL JSON and test document matches.
/// </summary>
public sealed class ObjectQueryEngine(IObjectQueryAstParser parser, IJsonQueryEvaluator evaluator)
{
    /// <summary>
    /// Creates an engine using the default parser and JSON evaluator implementations.
    /// </summary>
    public static ObjectQueryEngine CreateDefault()
    {
        return new ObjectQueryEngine(new ObjectQueryAstParser(), new JsonQueryEvaluator());
    }

    /// <summary>
    /// Parses raw query JSON text into an AST.
    /// </summary>
    public QueryNode ParseToAst(string queryJson)
    {
        return parser.Parse(queryJson);
    }

    /// <summary>
    /// Parses a Corvus-generated query type into an AST.
    /// </summary>
    public QueryNode ParseToAst(ObjectQueryDslV1Schema query)
    {
        return parser.Parse(query);
    }

    /// <summary>
    /// Parses and evaluates a query object against a JSON document root.
    /// </summary>
    public bool IsMatch(ObjectQueryDslV1Schema query, JsonElement document)
    {
        var ast = parser.Parse(query);
        return evaluator.IsMatch(ast, document);
    }

    /// <summary>
    /// Parses and evaluates a raw query JSON string against a JSON document root.
    /// </summary>
    public bool IsMatch(string queryJson, JsonElement document)
    {
        var ast = parser.Parse(queryJson);
        return evaluator.IsMatch(ast, document);
    }

    /// <summary>
    /// Parses and evaluates a query object against a JSON document root.
    /// </summary>
    public bool IsMatch(QueryNode queryAst, JsonElement document)
    {
        return evaluator.IsMatch(queryAst, document);
    }
}
