using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Join entity mapping a metadata key on a data entity (dataset or data file) to a metadata JSON field.
/// </summary>
public class DataEntityMetadataJsonField
{
    public Guid Id { get; init; } = Guid.NewGuid();

    internal Guid? DataSetId { get; set; }
    internal DataSet? DataSet { get; set; }

    internal Guid? DataFileId { get; set; }
    internal DataFile? DataFile { get; set; }

    public string MetadataKey { get; set; } = string.Empty;

    public Guid MetadataJsonFieldId { get; set; }
    public required MetadataJsonField MetadataJsonField { get; set; }

    [NotMapped]
    public IUniqueEntity? DataEntity => (IUniqueEntity?) DataSet ?? DataFile;

    [NotMapped]
    public Guid? DataEntityId => DataSetId ?? DataFileId;
}
