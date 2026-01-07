using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

public class MetaDataCollectionColumn : IUniqueEntityWithParent
{
    private string _metadataKey = string.Empty;

    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid ParentCollectionId { get; set; }
    public DataCollectionEntity? ParentCollection { get; set; }
    [NotMapped]
    public Guid? ParentId => ParentCollectionId;

    public string MetadataKey
    {
        get => _metadataKey;
        set => _metadataKey = value.ToLower();
    }

    public required Guid SchemaId { get; set; }
    public JsonSchemaEntity? Schema { get; set; }

    public Guid? DefaultFieldId { get; set; }
    /// <summary>
    /// Reference to the default field, that data sets will inherit from the collection.
    /// </summary>
    public MetadataJsonField? DefaultField { get; set; }
}