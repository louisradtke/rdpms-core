namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// A dataset is a collection of data files. The workflow is: a worker creates a dataset, then uploads the associated
/// files, and finally seals the dataset. Only sealed datasets can be used for further processing.
/// </summary>
/// <param name="Name"></param>
public record DataSetEntity(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// List of all data sets influencing this data set. Those may also be data sets from other instances, why this is
    /// not directly referring other data sets. If this is empty, and there is no <see cref="CreateJob"/>, then this
    /// data set contains raw data, that has not been processed by the system.
    /// </summary>
    public required List<Guid> AncestorDatasetIds { get; set; }
    
    /// <summary>
    /// Non-unique name of a dataset
    /// </summary>
    public string Name { get; set; } = Name;

    public List<DataFileEntity> Files { get; set; } = [];
    public List<TagEntity> AssignedTags { get; set; } = [];
    public DateTime CreatedStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Respective stamp in <b>UTC</b>. If this file was deleted, the value represents the deletion stamp.
    /// false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }

    public DataSetStateEntity State { get; set; } = DataSetStateEntity.Uninitialized;

    /// <summary>
    /// If data set was created by a job, refer it
    /// </summary>
    public required JobEntityEntity? CreateJob { get; init; }

    /// <summary>
    /// List of all jobs using this dataset
    /// </summary>
    public List<JobEntityEntity> SourceForJobs { get; set; } = [];

    /// <summary>
    /// Maps a string/name (unique per set) to a JSON field
    /// </summary>
    public List<MetadataJsonFieldEntity> MetadataJsonFields { get; set; } = [];
}
