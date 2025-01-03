namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Instance of a job
/// </summary>
public record Job(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Instance-wide job identifier. Counter is managed by database.
    /// </summary>
    public UInt64 LocalId { get; set; }
    
    public string Name { get; set; } = Name;

    /// <summary>
    /// State of the job.
    /// </summary>
    public JobState State { get; set; } = JobState.Created;
    
    /// <summary>
    /// Time stamp when the last update came in (important for sanitization).
    /// </summary>
    public DateTime LastUpdateStamp { get; set; }
    
    public DateTime CreatedStamp { get; set; } = DateTime.UtcNow;
    public DateTime? StartedStamp { get; set; }
    public DateTime? TerminatedStamp { get; set; }
    public List<DataSet> SourceDatasets {get; set; } = [];
    public List<DataSet> OutputDatasets { get; set; } = [];
    public List<LogSection> Logs { get; set; } = [];
    
    /// <summary>
    /// Override the <see cref="DataStore"/> set by OutputContainer.
    /// </summary>
    public DataStore? OutputDataStore { get; set; }
    
    /// <summary>
    /// With which container to associate the file 
    /// </summary>
    public required DataContainer OutputContainer { get; init; }
}
