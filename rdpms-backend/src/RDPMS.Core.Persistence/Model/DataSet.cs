namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// A dataset is a collection of data files. The workflow is: a worker creates a dataset, then uploads the associated
/// files, and finally seals the dataset. Only sealed datasets can be used for further processing.
/// </summary>
/// <param name="name"></param>
public class DataSet(string name) : IUniqueEntity, IUniqueEntityWithParent
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// List of all data sets influencing this data set. Those may also be data sets from other instances, why this is
    /// not directly referring other data sets. If this is empty, and there is no <see cref="CreateJob"/>, then this
    /// data set contains raw data, that has not been processed by the system.
    /// </summary>
    public List<Guid> AncestorDatasetIds { get; set; } = [];

    /// <summary>
    /// Non-unique name of a dataset
    /// </summary>
    public string Name { get; set; } = name;
    
    /// <summary>
    /// Id of the parent <see cref="DataCollectionEntity"/>. Nullability is a convenience feature, every dataset should
    /// have a parent collection.
    /// </summary>
    public Guid? ParentId { get; set; }

    public List<DataFile> Files { get; set; } = [];
    public List<Tag> AssignedTags { get; set; } = [];
    public List<Label> AssignedLabels { get; set; } = [];
    public DateTime CreatedStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Respective stamp in <b>UTC</b>. If this file was deleted, the value represents the deletion stamp.
    /// false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }

    public DataSetState State { get; set; } = DataSetState.Uninitialized;

    /// <summary>
    /// If data set was created by a job, refer it
    /// </summary>
    public Job? CreateJob { get; init; }

    /// <summary>
    /// List of all jobs using this dataset
    /// </summary>
    public List<Job> SourceForJobs { get; set; } = [];

    /// <summary>
    /// Maps a string/name (unique per dataset) to a JSON field
    /// </summary>
    public List<MetadataJsonField> MetadataJsonFields { get; set; } = [];
}
