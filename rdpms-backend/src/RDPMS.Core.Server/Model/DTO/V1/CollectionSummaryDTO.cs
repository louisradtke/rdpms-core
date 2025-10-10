namespace RDPMS.Core.Server.Model.DTO.V1;

public record CollectionSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Slug { get; set; }
    public string? Name { get; set; }
    public int? DataSetCount { get; set; } = 0;
    public Guid? DefaultDataStoreId { get; set; }
    public Guid? ProjectId { get; set; }
}