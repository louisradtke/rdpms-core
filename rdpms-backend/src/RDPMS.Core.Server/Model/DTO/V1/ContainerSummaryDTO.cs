namespace RDPMS.Core.Server.Model.DTO.V1;

public record CollectionSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public int? DataFilesCount { get; set; } = 0;
    public Guid? DefaultDataStoreId { get; set; }
}