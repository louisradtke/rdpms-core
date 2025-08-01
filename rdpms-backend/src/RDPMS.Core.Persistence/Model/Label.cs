namespace RDPMS.Core.Persistence.Model;

public class Label : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public string Name { get; init; } = string.Empty;
    
    public required Project ParentProject { get; init; }

    /// <summary>
    /// List of all DataSets that are assigned this label
    /// </summary>
    public List<DataSet> AssignedToDataSets { get; set; } = [];
}
