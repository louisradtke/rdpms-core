namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Enum indicating whether a dataset was just created or all associated files
/// were linked. Remember design decision: datasets shall be immutable.
/// </summary>
public enum DataSetState
{
    /// <summary>
    /// Data set was created, but not sealed yet.
    /// </summary>
    Uninitialized = 0,

    /// <summary>
    /// Data set is sealed, and a defined set of checks was performed.
    /// </summary>
    Sealed = 1,
}