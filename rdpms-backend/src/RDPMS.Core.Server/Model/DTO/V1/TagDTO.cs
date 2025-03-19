namespace RDPMS.Core.Server.Model.DTO.V1;

public record TagDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
}