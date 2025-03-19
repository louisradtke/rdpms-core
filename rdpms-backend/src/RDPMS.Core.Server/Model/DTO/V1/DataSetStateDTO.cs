namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Enum indicating whether a dataset was just created or all associated files
/// were linked. Remember design decision: datasets shall be immutable.
/// </summary>
public enum DataSetStateDTO
{
    Uninitialized = 0,
    Sealed = 1
}