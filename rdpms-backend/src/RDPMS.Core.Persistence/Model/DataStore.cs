namespace RDPMS.Core.Persistence.Model;

public record DataStore(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Name of the data store
    /// </summary>
    public string Name { get; set; } = Name;

    /// <summary>
    /// List of all files stored on this instance
    /// </summary>
    public List<DataFile> DataFiles { get; set; } = [];
}
