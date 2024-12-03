namespace RDPMS.Core.Persistence.Model;

public record DataFile(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = Name;
    public required ContentType FileType { get; set; }
    
    /// <summary>
    /// Respective stamp in <b>UTC</b>
    /// </summary>
    public DateTime CreationStamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this file represents multiple data points, each being associated with a timestamp. e.g. MCAP or MP4
    /// </summary>
    public required bool IsTimeSeries { get; set; }
    
    /// <summary>
    /// The stamp of the first data point, if <see cref="IsTimeSeries"/> is true.
    /// </summary>
    public DateTime? BeginStamp { get; set; }

    /// <summary>
    /// Respective stamp in <b>UTC</b>. If this file was deleted, the value represents the deletion stamp.
    /// false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }
    
    /// <summary>
    /// Whether this file was deleted
    /// </summary>
    /// <returns>true if DeletedStamp != null, false otherwise</returns>
    public bool IsDeleted() => DeletedStamp != null;
}
