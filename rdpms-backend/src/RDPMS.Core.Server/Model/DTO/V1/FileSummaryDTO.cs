namespace RDPMS.Core.Server.Model.DTO.V1;

public class FileSummaryDTO
{
    public Guid Id { get; set; } = Guid.Empty;
    public string Name { get; set; } = string.Empty;
    public required ContentTypeDTO ContentType { get; set; }
    public long Size { get; set; } = 0;
    public DateTime CreationStamp { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedStamp { get; set; }
    public DateTime? BeginStamp { get; set; }
    public double? Duration { get; set; }
}