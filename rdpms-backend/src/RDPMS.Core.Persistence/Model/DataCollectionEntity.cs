using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Since data sets are not bound to their storage, they can be virtually put into a collection.
/// </summary>
public class DataCollectionEntity(string name) : IUniqueEntityWithSlugAndParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ParentProjectId { get; set; }
    public Project? ParentProject { get; set; }

    /// <summary>
    /// Id of the parent <see cref="Project"/>. Nullability is a convenience feature, every collection should have a
    /// parent project.
    /// </summary>
    [NotMapped]
    public Guid? ParentId => ParentProjectId;

    public string? Slug { get; set; }

    public string Name { get; set; } = name;

    public string? Description { get; set; }

    /// <summary>
    /// List of all data sets this collection holds
    /// </summary>
    public List<DataSet> ContainedDatasets { get; set; } = [];

    /// <summary>
    /// When creating a new dataset or adding files, this is where to put it by default
    /// </summary>
    public DataStore? DefaultDataStore { get; set; }
    public Guid? DefaultDataStoreId { get; set; }

    public List<MetaDataCollectionColumn> MetaDataColumns { get; set; } = [];
    
    // my first idea was to use the label for filtering meta data, but we'll do it here for now.
    // public Label? DefaultLabel { get; set; }
    // public Guid? DefaultLabelId { get; set; }
}