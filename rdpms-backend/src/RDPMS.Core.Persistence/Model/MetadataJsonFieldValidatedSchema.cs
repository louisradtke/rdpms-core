namespace RDPMS.Core.Persistence.Model;

public class MetadataJsonFieldValidatedSchema
{
    public required Guid MetadataJsonFieldId { get; set; }
    public required Guid JsonSchemaEntityId { get; set; }
}