namespace RDPMS.Core.Persistence.Model;

public class JsonSchemaEntity : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string SchemaId { get; set; } = string.Empty;
    public string SchemaString { get; set; } = string.Empty;
}