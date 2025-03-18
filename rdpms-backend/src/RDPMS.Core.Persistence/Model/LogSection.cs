namespace RDPMS.Core.Persistence.Model;

// personal note: in the log views in RBB, when looking at the output of some script, the stderr stream was often not
// present but only stdout was displayed. The missing information yet often was necessary. The current system is not
// able to display this information.

public class LogSection
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The name of e.g. the process, that hat put this out. May be empty.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>
    /// The content of the current log. null, if moved to data store.
    /// </summary>
    public string? LogContent { get; set; }
    
    /// <summary>
    /// The content of the log file, if moved to a store (to keep db small).
    /// </summary>
    public DataFile? StoredFile { get; set; }
}
