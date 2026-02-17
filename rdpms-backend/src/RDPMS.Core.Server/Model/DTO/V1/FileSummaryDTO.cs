namespace RDPMS.Core.Server.Model.DTO.V1;

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

    /// <summary>
    /// Storage references of the file.
    /// Null means this information is not included.
    /// Empty means this file currently has no references.
    /// </summary>
    public List<FileStorageReferenceSummaryDTO>? References { get; set; }
}
