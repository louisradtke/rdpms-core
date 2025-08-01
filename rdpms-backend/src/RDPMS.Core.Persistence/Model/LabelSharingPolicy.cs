namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Class indicating whether some project can access labels from another project.
/// </summary>
public class LabelSharingPolicy
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// The label (having one parent project) being shared to another project.
    /// </summary>
    public required Label SharedLabel { get; init; }
    
    /// <summary>
    /// The project, the label was shared with.
    /// </summary>
    public required Project ProjectSharedTo { get; init; }
    
    /// <summary>
    /// Whether datasets can be read from the label
    /// </summary>
    public bool CanRead { get; set; }

    /// <summary>
    /// Whether datasets can be added to the label.
    /// </summary>
    public bool CanWrite { get; set; }
}