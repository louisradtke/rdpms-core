namespace RDPMS.Core.Persistence.Model;

public class Slug : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Value { get; set; } = string.Empty;
    public Guid EntityId { get; set; } = Guid.Empty;
    public SlugState State { get; set; } = SlugState.Active;
    public SlugType? Type { get; set; }
}

public enum SlugState
{
    Active = 0,
    Deprecated = 1
}

public enum SlugType
{
    Project = 0,
    DataCollection = 1,
    DataSet = 2,
    DataStore = 3
}