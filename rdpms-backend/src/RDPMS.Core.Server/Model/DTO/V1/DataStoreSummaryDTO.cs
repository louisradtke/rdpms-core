namespace RDPMS.Core.Server.Model.DTO.V1;

public record DataStoreSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public int? FilesCount { get; set; }
    public string? StorageType { get; set; }
    public string? PropertiesJson { get; set; }
}