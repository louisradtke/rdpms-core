using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// A dataset is a collection of data files. The workflow is: a worker creates a dataset, then uploads the associated
/// files, and finally seals the dataset. Only sealed datasets can be used for further processing.
/// </summary>
/// <param name="name"></param>
public class DataSet(string name) : IUniqueEntity, IUniqueEntityWithSlugAndParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ParentCollectionId { get; set; }
    public DataCollectionEntity? ParentCollection { get; set; }

    /// <summary>
    /// Id of the parent <see cref="DataCollectionEntity"/>. Nullability is a convenience feature, every dataset should
    /// have a parent collection.
    /// </summary>
    [NotMapped]
    public Guid? ParentId => ParentCollectionId;


    public string? Slug { get; set; }

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
    
    public List<DataFile> Files { get; set; } = [];
    public List<Tag> AssignedTags { get; set; } = [];
    public List<Label> AssignedLabels { get; set; } = [];
    public DateTime CreatedStamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Stamp in <b>UTC</b>, where the dataset was selected for deletion
    /// (leaving <see cref="Model.DeletionState.Active"/>).
    /// </summary>
    public DateTime? DeletedStamp { get; set; }

    /// <summary>
    /// State indicating, whether the data set was initialized/verified.
    /// </summary>
    public DataSetState LifecycleState { get; set; } = DataSetState.Uninitialized;

    /// <summary>
    /// State indicating, whether the data set was deleted.
    /// </summary>
    public DeletionState DeletionState { get; set; } = DeletionState.Active;

    /// <summary>
    /// If data set was created by a job, refer it
    /// </summary>
    public Job? CreateJob { get; init; }

    /// <summary>
    /// List of all jobs using this dataset
    /// </summary>
    public List<Job> SourceForJobs { get; set; } = [];

    /// <summary>
    /// Metadata fields associated with this dataset, including their dataset-specific key.
    /// </summary>
    public List<DataEntityMetadataJsonField> MetadataJsonFields { get; set; } = [];

    public IReadOnlyDictionary<string, MetadataJsonField> Metadata 
    {
        get {
            // todo: not very efficient, return value calculated on every access
            var dict = new Dictionary<string, MetadataJsonField>();
            foreach (var field in MetadataJsonFields)
            {
                dict[field.MetadataKey] = field.Field;
            }

            return dict.AsReadOnly();
        }
    }
}
