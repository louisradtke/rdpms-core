namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileCreateResponseDTO
{
    public string? UploadUri { get; set; }
    public Guid? FileId { get; set; }
}