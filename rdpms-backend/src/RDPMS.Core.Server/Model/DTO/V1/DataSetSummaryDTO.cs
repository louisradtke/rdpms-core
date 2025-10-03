namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Represents a summary of a dataset, including identifying information, timestamps, state, tags,
/// and metadata fields.
/// </summary>
public record DataSetSummaryDTO
{
    /// <summary>
    /// Uniquely identifies the dataset. Typically server-generated. Should not be manually set by the client.
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// Non-unique, mandatory descriptive name of the dataset. Must be provided by the client.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// List of tags associated with the dataset, used for categorization and filtering purposes.
    /// </summary>
    public List<TagDTO>? AssignedTags { get; set; }

    /// <summary>
    /// UTC timestamp when the dataset was originally created.
    /// Mandatory property, should be provided by the client.
    /// </summary>
    public DateTime? CreatedStampUTC { get; set; }

    /// <summary>
    /// UTC timestamp that indicates when the dataset was deleted.
    /// Null if the dataset has not been deleted yet.
    /// </summary>
    public DateTime? DeletedStampUTC { get; set; }

    /// <summary>
    /// UTC timestamp that marks the period begin of the dataset.
    /// Only to be set by server.
    /// </summary>
    public DateTime? BeginStampUTC { get; set; }

    /// <summary>
    /// UTC timestamp that marks the period end of the dataset.
    /// Only to be set by server.
    /// </summary>
    public DateTime? EndStampUTC { get; set; }

    /// <summary>
    /// Indicates, whether the dataset (and its files) are immutable.
    /// Only to be set by server.
    /// </summary>
    public DataSetStateDTO? State { get; set; }

    /// <summary>
    /// Indicates if the dataset represents time-series data.
    /// Only to be set by server.
    /// </summary>
    public bool? IsTimeSeries { get; set; }

    /// <summary>
    /// Flags whether the dataset has been marked as deleted.
    /// Only to be set by server.
    /// </summary>
    public bool? IsDeleted { get; set; }

    /// <summary>
    /// Fields, for which metadata exists.
    /// Only to be set by server.
    /// </summary>
    public List<string>? MetadataFields { get; set; }
    
    /// <summary>
    /// Amount of files in the dataset.
    /// </summary>
    public int FileCount { get; set; }
}