namespace RDPMS.Core.Persistence.Model;

public class Project(string name) : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; set; } = name;

    public string Description { get; set; } = string.Empty;
    
    public List<Label> Labels { get; set; } = [];
    
    public List<LabelSharingPolicy> SharedLabels { get; set; } = [];
    
    public List<ContentType> AllFileTypes { get; set; } = [];

    /// <summary>
    /// When creating a new dataset or adding files, this is where to put it by default
    /// </summary>
    public DataStore? DefaultDataStore { get; set; }
    
    /// <summary>
    /// All collections assigned to this project.
    /// </summary>
    public List<DataCollectionEntity> DataCollections { get; set; } = [];
    
    /// <summary>
    /// All data stores assigned to this project.
    /// </summary>
    public List<DataStore> DataStores { get; set; } = [];
}
