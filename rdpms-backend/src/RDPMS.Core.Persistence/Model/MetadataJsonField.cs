namespace RDPMS.Core.Persistence.Model;

public class MetadataJsonField
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required DataFile Value { get; set; }
    public List<JsonSchemaEntity> ValidatedSchemas { get; set; } = [];
}
