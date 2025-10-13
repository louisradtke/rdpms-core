namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileCreateRequestDTO
{
    public string? Name { get; set; }
    public Guid? ContentTypeId { get; set; }
    public long? SizeBytes { get; set; }
    public string? PlainSHA256Hash { get; set; }
    public DateTime? CreatedStamp { get; set; }
    public DateTime? BeginStamp { get; set; }
    public DateTime? EndStamp { get; set; }
}