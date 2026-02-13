using System.Text.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine.Ast;

namespace RDPMS.Core.QueryEngine.Parsing;

public interface IObjectQueryAstParser
{
    QueryNode Parse(string queryJson);

    QueryNode Parse(JsonElement queryJson);

    QueryNode Parse(ObjectQueryDslV1Schema query);
}
