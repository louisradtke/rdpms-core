namespace RDPMS.Core.Persistence.Model;

public record DataFile(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The instance that issued the job that created this file
    /// </summary>
    public required Guid OriginInstanceId { get; init; }

    public string Name { get; set; } = Name;
    public required ContentType FileType { get; set; }
    public DateTime CreationStamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// If this file was deleted, the value represents the deletion stamp. false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }
    
    /// <summary>
    /// Whether this file was deleted
    /// </summary>
    /// <returns>true if DeletedStamp != null, false otherwise</returns>
    public bool IsDeleted() => DeletedStamp != null;
}
