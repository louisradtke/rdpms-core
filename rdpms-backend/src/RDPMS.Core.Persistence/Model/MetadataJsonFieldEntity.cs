namespace RDPMS.Core.Persistence.Model;

public record MetadataJsonFieldEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Key { get; init; }
    public required string Value { get; set; }
}
