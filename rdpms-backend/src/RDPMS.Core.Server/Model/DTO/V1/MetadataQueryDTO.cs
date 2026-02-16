using System.Text.Json;

namespace RDPMS.Core.Server.Model.DTO.V1;

public record MetadataQueryDTO()
{
    public QueryMode Mode { get; set; } = QueryMode.And;
    public List<MetadataQueryPartDTO> Queries { get; set; } = [];
}

public record MetadataQueryPartDTO()
{
    public JsonElement Query { get; set; }
    public string MetadataKey { get; set; } = string.Empty;
    public MetadataColumnTargetDTO Target { get; set; } = MetadataColumnTargetDTO.Dataset;
}

public enum QueryMode
{
    And = 0,
    Or = 1
}
