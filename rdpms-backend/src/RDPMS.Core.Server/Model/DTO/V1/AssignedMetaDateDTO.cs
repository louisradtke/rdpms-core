namespace RDPMS.Core.Server.Model.DTO.V1;

public record AssignedMetaDateDTO
{
    /// <summary>
    /// Key linking an entity to a meta date.
    /// </summary>
    public string? MetadataKey { get; set; }

    /// <summary>
    /// Id of the meta date.
    /// </summary>
    public Guid? MetadataId { get; set; }
    
    /// <summary>
    /// The meta date itself, optionally to be set.
    /// </summary>
    public MetaDateDTO? Field { get; set; }

    /// <summary>
    /// Whether this meta date is inherited from the parent collection.
    /// </summary>
    public bool? Inherited { get; set; }

    /// <summary>
    /// Whether the schema, the collection declares for this key, has been verified.
    /// </summary>
    public bool? CollectionSchemaVerified { get; set; }
}
