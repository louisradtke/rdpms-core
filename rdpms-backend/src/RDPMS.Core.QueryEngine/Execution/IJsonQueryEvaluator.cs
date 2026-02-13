using System.Text.Json;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Execution;

public interface IJsonQueryEvaluator
{
    bool IsMatch(QueryNode query, JsonElement document);
}
