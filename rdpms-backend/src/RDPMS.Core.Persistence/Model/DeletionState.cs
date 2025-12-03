namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Enum indicating whether an entity was deleted.
/// </summary>
public enum DeletionState
{
    /// <summary>
    /// Entity is untouched.
    /// </summary>
    None = 0,

    /// <summary>
    /// Entity was selected for deletion, but this is still revocable.
    /// </summary>
    DeletionPending = 1,
    
    /// <summary>
    /// Entity is permanently deleted.
    /// </summary>
    Deleted = 2,
}