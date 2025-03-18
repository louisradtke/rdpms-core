namespace RDPMS.Core.Server.Model.DTO.V1;

public class DataFileCreateRequestDTO
{
    public string? Name { get; set; }
    public Guid? ContentTypeId { get; set; }
    public Guid? AssociatedDataSetId { get; set; }
    public long? Size { get; set; }
    public string? Hash { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? BeginStamp { get; set; }
    public DateTime? EndStamp { get; set; }
}