namespace RDPMS.Core.Persistence.Model;

public class DataStore(string name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Name of the data store
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// List of all files stored on this instance
    /// </summary>
    public List<DataFile> DataFiles { get; set; } = [];
}
