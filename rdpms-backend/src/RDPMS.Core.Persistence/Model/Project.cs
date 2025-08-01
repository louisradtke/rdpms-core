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
    public required DataStore DefaultDataStore { get; set; }
}
