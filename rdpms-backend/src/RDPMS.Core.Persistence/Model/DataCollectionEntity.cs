namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Since data sets are not bound to their storage, they can be virtually put into a collection.
/// </summary>
public class DataCollectionEntity(string name) : IUniqueEntity, IUniqueEntityWithParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; } = name;

    /// <summary>
    /// Id of the parent <see cref="Project"/>. Nullability is a convenience feature, every collection should have a
    /// parent project.
    /// </summary>
    public Guid? ParentId { get; set; }
    
    /// <summary>
    /// List of all data sets this collection holds
    /// </summary>
    public List<DataSet> ContainedDatasets { get; set; } = [];

    /// <summary>
    /// When creating a new dataset or adding files, this is where to put it by default
    /// </summary>
    public DataStore? DefaultDataStore { get; set; }
}