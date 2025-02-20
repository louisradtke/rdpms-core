namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Instance of a job
/// </summary>
public record JobEntityEntity(string Name)
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
    public JobStateEntity State { get; set; } = JobStateEntity.Created;
    
    /// <summary>
    /// Time stamp when the last update came in (important for sanitization).
    /// </summary>
    public DateTime LastUpdateStamp { get; set; }
    
    public DateTime CreatedStamp { get; set; } = DateTime.UtcNow;
    public DateTime? StartedStamp { get; set; }
    public DateTime? TerminatedStamp { get; set; }
    public List<DataSetEntity> SourceDatasets {get; set; } = [];
    public List<DataSetEntity> OutputDatasets { get; set; } = [];
    public List<LogSectionEntity> Logs { get; set; } = [];
    
    /// <summary>
    /// Override the <see cref="DataStoreEntity"/> set by OutputContainer.
    /// </summary>
    public DataStoreEntity? OutputDataStore { get; set; }
    
    /// <summary>
    /// With which container to associate the file 
    /// </summary>
    public required DataContainerEntity OutputContainer { get; init; }
}
