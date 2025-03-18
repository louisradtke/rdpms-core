namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Enum indicating whether a dataset was just created or all associated files
/// were linked. Remember design decision: datasets shall be immutable.
/// </summary>
public enum DataSetState
{
    Uninitialized = 0,
    Sealed = 1
}