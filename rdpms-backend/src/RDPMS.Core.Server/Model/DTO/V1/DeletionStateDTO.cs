using System.ComponentModel;

namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Enum indicating whether an entity was deleted.
/// </summary>
public enum DeletionStateDTO
{
    /// <summary>
    /// Entity is untouched.
    /// </summary>
    Active = 0,

    /// <summary>
    /// Entity was selected for deletion, but this is still revocable.
    /// </summary>
    DeletionPending = 1,
    
    /// <summary>
    /// Entity is permanently deleted.
    /// </summary>
    Deleted = 2,
}