using System.Text.Json;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Execution;

/// <summary>
/// Evaluates a parsed query AST against a JSON document instance.
/// </summary>
public interface IJsonQueryEvaluator
{
    bool IsMatch(QueryNode query, JsonElement document);
}
