namespace RDPMS.Core.Server.Model.DTO.V1;

public class DataSetSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public List<TagDTO>? AssignedTags { get; set; }
    public DateTime? CreatedStampUTC { get; set; }
    public DateTime? DeletedStampUTC { get; set; }
    public DataSetStateDTO? State { get; set; }
    public DateTime? BeginStampUTC { get; set; }
    public bool? IsTimeSeries { get; set; }
}