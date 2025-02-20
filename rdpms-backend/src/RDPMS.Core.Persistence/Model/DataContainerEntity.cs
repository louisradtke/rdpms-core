namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Since data sets are not bound to their storage, they can be virtually put into a container.
/// </summary>
public record DataContainerEntity(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; } = Name;

    /// <summary>
    /// List of all data sets this container holds
    /// </summary>
    public List<DataSetEntity> AssociatedDataSets { get; set; } = [];

    /// <summary>
    /// When creating a new dataset or adding files, this is where to put it by default
    /// </summary>
    public required DataStoreEntity DefaultDataStore { get; set; }
}
