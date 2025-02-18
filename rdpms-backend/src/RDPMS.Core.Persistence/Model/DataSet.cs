namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// A dataset is a collection of data files. The workflow is: a worker creates a dataset, then uploads the associated
/// files (so that we never have files lying around, where we don't know their origin, in case a worker dies while
/// uploading), and finally seals the dataset. Only sealed datasets can be used for further processing.
/// </summary>
/// <param name="Name"></param>
public record DataSet(string Name)
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

    public List<DataFile> Files { get; set; } = [];
    public List<Tag> AssignedTags { get; set; } = [];
    public DateTime CreateStamp { get; set; } = DateTime.UtcNow;
    
    public DataSetState State { get; set; } = DataSetState.Uninitialized;

    /// <summary>
    /// If data set was created by a job, refer it
    /// </summary>
    public required Job? CreateJob { get; init; }

    /// <summary>
    /// List of all jobs using this dataset
    /// </summary>
    public List<Job> SourceForJobs { get; set; } = [];

    /// <summary>
    /// Maps a string/name (unique per set) to a JSON field
    /// </summary>
    public List<MetadataJsonField> MetadataJsonFields { get; set; } = [];
}
