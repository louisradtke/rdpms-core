using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

public class Tag(string name) : IUniqueEntityWithParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ParentProjectId { get; set; }

    /// <summary>
    /// Project the tag belongs to.
    /// </summary>
    public Project? ParentProject { get; set; }
    [NotMapped]
    public Guid? ParentId => ParentProjectId;

    public string Name { get; set; } = name;

    /// <summary>
    /// Datasets this tag is assigned to.
    /// </summary>
    public List<DataSet> AssignedToDataSets { get; set; } = [];
}
