namespace RDPMS.Core.Server.Model.DTO.V1;

public record MetaDateCollectionColumnDTO
{
    public string? MetadataKey { get; set; }
    public SchemaDTO? Schema { get; set; }
    public Guid? DefaultFieldId { get; set; }
}