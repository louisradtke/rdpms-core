namespace RDPMS.Core.Persistence;

public interface IUniqueEntityWithNullableParent : IUniqueEntity
{
    public Guid? ParentId { get; }
}