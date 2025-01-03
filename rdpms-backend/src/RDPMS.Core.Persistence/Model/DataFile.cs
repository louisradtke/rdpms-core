using System.ComponentModel.DataAnnotations.Schema;

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
    /// The stamp of the first data point, if <see cref="IsTimeSeries"/> is true.
    /// </summary>
    public DateTime? BeginStamp { get; init; }

    /// <summary>
    /// Respective stamp in <b>UTC</b>. If this file was deleted, the value represents the deletion stamp.
    /// false otherwise.
    /// </summary>
    public DateTime? DeletedStamp { get; set; }
    
    /// <summary>
    /// Read-only property indicating, whether this file represents multiple data points, each being associated with a
    /// timestamp. e.g. MCAP or MP4. true if <see cref="BeginStamp"/> != null
    /// </summary>
    public bool IsTimeSeries => BeginStamp != null;

    /// <summary>
    /// Read-only property indicating, whether this file was deleted. true if <see cref="DeletedStamp"/> != null
    /// </summary>
    public bool IsDeleted => DeletedStamp != null;
}
