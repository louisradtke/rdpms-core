namespace RDPMS.Core.Infra.AppInitialization;

/// <summary>
/// Registration mode for <see cref="AutoRegisterAttribute"/>.
/// </summary>
[Flags]
public enum RegisterFlags
{
    /// <summary>
    /// Register the implementation for all of its direct (shallow) interfaces.
    /// This avoids binding inherited framework interfaces inadvertently.
    /// </summary>
    ShallowInterfaces = 0b_0001,

    /// <summary>
    /// Register the implementation type as itself, without binding any interfaces.
    /// </summary>
    Self = 0b_0010
}