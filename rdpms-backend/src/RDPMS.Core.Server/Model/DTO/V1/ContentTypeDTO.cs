namespace RDPMS.Core.Server.Model.DTO.V1;

public record ContentTypeDTO
{
    public Guid? Id { get; set; }
    public string? Abbreviation { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; } = string.Empty;
    public string? MimeType { get; set; }
}