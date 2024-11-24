namespace RDPMS.Core.Persistence.Model;

public record LogSection()
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The content of the current log. null, if moved to data store.
    /// </summary>
    public string? LogContent { get; set; }
    
    /// <summary>
    /// The content of the log file, if moved to a store (to keep db small).
    /// </summary>
    public DataFile? StoredFile { get; set; }
}
