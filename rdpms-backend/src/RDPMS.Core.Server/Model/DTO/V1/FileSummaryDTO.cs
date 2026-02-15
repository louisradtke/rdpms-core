using System.Text.Json.Serialization;

namespace RDPMS.Core.Server.Model.DTO.V1;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(FileSummaryDTO), "summary")]
[JsonDerivedType(typeof(FileMetadataSummaryDTO), "metadata")]
[JsonDerivedType(typeof(FileDetailedDTO), "detailed")]
public record FileSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? DownloadURI { get; set; }
    public ContentTypeDTO? ContentType { get; set; }
    public long? Size { get; set; }
    public DateTime? CreatedStampUTC { get; set; }
    public DateTime? DeletedStampUTC { get; set; }
    public DateTime? BeginStampUTC { get; set; }
    public DateTime? EndStampUTC { get; set; }
    public bool? IsTimeSeries { get; set; }
    
    /// <summary>
    /// Fields, for which metadata exists.
    /// Only to be set by server.
    /// </summary>
    public List<AssignedMetaDateDTO>? MetaDates { get; set; }

    /// <summary>
    /// Indicates, whether the dataset (and its files) are deleted or whether deletion is pending.
    /// Only to be set by server.
    /// Lifecycle is: None -> [DeletionPending ->] Deleted
    /// </summary>
    public DeletionStateDTO? DeletionState { get; set; }
}
