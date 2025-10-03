namespace RDPMS.Core.Infra.AppInitialization;

/// <summary>
/// Registration mode for <see cref="AutoRegisterAttribute"/>.
/// </summary>
public enum RegisterAs
{
    /// <summary>
    /// Register the implementation for all of its direct (shallow) interfaces.
    /// This avoids binding inherited framework interfaces inadvertently.
    /// </summary>
    AllInterfaces,

    /// <summary>
    /// Register the implementation type as itself, without binding any interfaces.
    /// </summary>
    SelfOnly,

    /// <summary>
    /// Register only for the interfaces listed in <see cref="AutoRegisterAttribute.SpecificInterfaces"/>.
    /// </summary>
    SpecificInterfaces,
}