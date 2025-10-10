namespace RDPMS.Core.Persistence;

public interface IUniqueEntityWithParent : IUniqueEntity
{
    public Guid? ParentId { get; }
}