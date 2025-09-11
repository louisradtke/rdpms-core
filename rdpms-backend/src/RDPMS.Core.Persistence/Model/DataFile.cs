using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Entity identifying a file, independent of its storage location.
/// </summary>
/// <param name="name"></param>
public class DataFile(string name) : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = name;
    public required ContentType FileType { get; set; }
    
    /// <summary>
    /// Size of the uncompressed file in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// SHA256-hash of the uncompressed file.
    /// </summary>
    public string SHA256Hash { get; set; } = string.Empty;
    
    /// <summary>
    /// Respective stamp in <b>UTC</b>
    /// </summary>
    public DateTime CreatedStamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Respective stamp in <b>UTC</b>. If this file was deleted, the value represents the deletion stamp.
    /// false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }
    
    /// <summary>
    /// The stamp of the first data point, also indicating whether this is a time series. Null otherwise.
    /// </summary>
    public DateTime? BeginStamp { get; init; }

    /// <summary>
    /// If <see cref="BeginStamp"/> != null and if known, then this field contains the end timestamp. Else, it
    /// is null.
    /// </summary>
    public DateTime? EndStamp { get; init; }
    
    /// <summary>
    /// Read-only property indicating, whether this file represents multiple data points, each being associated with a
    /// timestamp. e.g. MCAP or MP4. true if <see cref="BeginStamp"/> != null
    /// </summary>
    public bool IsTimeSeries => BeginStamp != null;

    /// <summary>
    /// Read-only property indicating, whether this file was deleted. true if <see cref="DeletedStamp"/> != null
    /// </summary>
    public bool IsDeleted => DeletedStamp != null;

    /// <summary>
    /// References to the storage location(s) of this file.
    /// </summary>
    public List<FileStorageReference> Locations { get; set; } = new();
}
