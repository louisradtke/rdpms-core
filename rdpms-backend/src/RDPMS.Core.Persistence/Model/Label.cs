using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

public class Label : IUniqueEntity, IUniqueEntityWithParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ParentProjectId { get; set; }
    public Project? ParentProject { get; set; }
    [NotMapped]
    public Guid? ParentId => ParentProjectId;

    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// List of all DataSets that are assigned this label
    /// </summary>
    public List<DataSet> AssignedToDataSets { get; set; } = [];
}
