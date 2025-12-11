namespace RDPMS.Core.Persistence.Model;

public class Tag(string name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = name;

    /// <summary>
    /// Project the tag belongs to.
    /// </summary>
    public required Project ParentProject { get; init; }

    /// <summary>
    /// Datasets this tag is assigned to.
    /// </summary>
    public List<DataSet> AssignedToDataSets { get; set; } = [];
}
