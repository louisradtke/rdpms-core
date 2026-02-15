namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileDetailedDTO : FileSummaryDTO
{
    public List<FileStorageReferenceSummaryDTO>? References { get; set; }
}
