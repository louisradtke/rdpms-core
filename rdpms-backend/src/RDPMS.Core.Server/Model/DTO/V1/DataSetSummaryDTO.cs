namespace RDPMS.Core.Server.Model.DTO.V1;

public class DataSetSummaryDTO
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public List<TagDTO> AssignedTags { get; set; } = [];
    public DateTime CreatedStampUTC { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedStampUTC { get; set; }
    public DataSetStateDTO State { get; set; } = DataSetStateDTO.Uninitialized;
    public DateTime? BeginStampUTC { get; set; }
    public bool IsTimeSeries { get; set; }
}