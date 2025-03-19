namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public ContentTypeDTO? ContentType { get; set; }
    public long? Size { get; set; }
    public DateTime? CreatedStampUTC { get; set; }
    public DateTime? DeletedStampUTC { get; set; }
    public DateTime? BeginStampUTC { get; set; }
    public DateTime? EndStampUTC { get; set; }
    public bool? IsTimeSeries { get; set; }
    public bool? IsDeleted { get; set; }
}