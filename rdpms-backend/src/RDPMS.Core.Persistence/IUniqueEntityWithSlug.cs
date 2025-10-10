namespace RDPMS.Core.Persistence;

public interface IUniqueEntityWithSlug : IUniqueEntity
{
    public string? Slug { get; }
}