using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Join entity mapping a metadata key on a data entity (dataset or data file) to a metadata JSON field.
/// </summary>
public class DataEntityMetadataJsonField
{
    private string _metadataKey = string.Empty;
    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid? DataSetId { get; set; }
    public DataSet? DataSet { get; set; }

    public Guid? DataFileId { get; set; }
    public DataFile? DataFile { get; set; }

    public string MetadataKey
    {
        get => _metadataKey;
        set => _metadataKey = value.ToLower();
    }

    public Guid MetadataJsonFieldId { get; set; }
    public required MetadataJsonField MetadataJsonField { get; set; }

    [NotMapped]
    public IUniqueEntity? DataEntity
    {
        get => (IUniqueEntity?) DataSet ?? DataFile;
        // set
        // {
        //     if (value is DataSet ds)
        //     {
        //         DataSetId = ds.Id;
        //         DataSet = ds;
        //     }
        //     else if (value is DataFile df)
        //     {
        //         DataFileId = df.Id;
        //         DataFile = df;
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Value must be a DataSet or a DataFile");
        //     }
        // }
    } 

    [NotMapped]
    public Guid? DataEntityId => DataSetId ?? DataFileId;
}
