using System.Text.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine.Ast;
using RDPMS.Core.QueryEngine.Execution;
using RDPMS.Core.QueryEngine.Parsing;

namespace RDPMS.Core.QueryEngine;

public sealed class ObjectQueryEngine(IObjectQueryAstParser parser, IJsonQueryEvaluator evaluator)
{
    public static ObjectQueryEngine CreateDefault()
    {
        return new ObjectQueryEngine(new ObjectQueryAstParser(), new JsonQueryEvaluator());
    }

    public QueryNode ParseToAst(string queryJson)
    {
        return parser.Parse(queryJson);
    }

    public QueryNode ParseToAst(ObjectQueryDslV1Schema query)
    {
        return parser.Parse(query);
    }

    public bool IsMatch(ObjectQueryDslV1Schema query, JsonElement document)
    {
        var ast = parser.Parse(query);
        return evaluator.IsMatch(ast, document);
    }

    public bool IsMatch(string queryJson, JsonElement document)
    {
        var ast = parser.Parse(queryJson);
        return evaluator.IsMatch(ast, document);
    }
}
