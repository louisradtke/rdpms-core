using System.Text.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Parsing;

/// <summary>
/// Parses query DSL JSON into a normalized AST representation that is independent of storage concerns.
/// </summary>
public interface IObjectQueryAstParser
{
    QueryNode Parse(string queryJson);

    QueryNode Parse(JsonElement queryJson);

    QueryNode Parse(ObjectQueryDslV1Schema query);
}
