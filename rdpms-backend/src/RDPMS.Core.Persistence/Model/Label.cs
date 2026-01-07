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

    // public List<MetaDataCollectionColumn> MetaDataColumns { get; set; } = [];
}