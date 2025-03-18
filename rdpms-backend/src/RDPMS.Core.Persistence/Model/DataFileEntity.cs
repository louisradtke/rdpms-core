using System.ComponentModel.DataAnnotations.Schema;

namespace RDPMS.Core.Persistence.Model;

public record DataFileEntity(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Name { get; set; } = Name;
    public required ContentTypeEntity FileType { get; set; }
    
    public long Size { get; set; }
    
    public string Hash { get; set; } = string.Empty;
    
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
}
