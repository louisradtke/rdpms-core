using Microsoft.Extensions.DependencyInjection;

namespace RDPMS.Core.Infra.AppInitialization;


/// <summary>
/// Marks a concrete class for automatic dependency injection registration during startup.
/// </summary>
/// <remarks>
/// Usage:
/// - Apply this attribute to concrete classes that you want to be auto-registered in the DI container.
/// - By default, the implementation is registered for all of its direct interfaces (shallow).
/// - You can override the lifetime and the registration behavior via the attribute parameters.
/// - Open-generic implementations and interfaces (e.g., <c>IExportMapper&lt;,&gt;</c>) are supported if <see cref="IncludeOpenGenerics"/> is true.
/// </remarks>
/// <example>
/// [AutoRegister(ServiceLifetime.Singleton)]
/// public class FileSummaryDTOMapper : IExportMapper&lt;DataFile, FileSummaryDTO&gt; { ... }
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AutoRegisterAttribute(
    ServiceLifetime lifetime = ServiceLifetime.Singleton,
    RegisterAs registerAs = RegisterAs.AllInterfaces,
    bool includeOpenGenerics = true,
    bool includeNonGenericInterfaces = true
) : Attribute
{
    /// <summary>
    /// The service lifetime to use for the registration (e.g., Singleton, Scoped, Transient).
    /// Defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; } = lifetime;

    /// <summary>
    /// Controls which service types the implementation is registered as.
    /// - <see cref="RegisterAs.AllInterfaces"/>: Register for all direct (shallow) interfaces implemented by the class (default).
    /// - <see cref="RegisterAs.SelfOnly"/>: Register only as its concrete type.
    /// - <see cref="RegisterAs.SpecificInterfaces"/>: Register only for the interfaces listed in <see cref="SpecificInterfaces"/>.
    /// </summary>
    public RegisterAs RegisterAs { get; } = registerAs;

    /// <summary>
    /// When true, enables registering open-generic service mappings for open-generic implementations,
    /// e.g., IExportMapper&lt;,&gt; → GenericMapper&lt;,&gt;.
    /// Has no effect for closed generic or non-generic types. Defaults to true.
    /// </summary>
    public bool IncludeOpenGenerics { get; } = includeOpenGenerics;

    /// <summary>
    /// When false, non-generic interfaces (e.g., IDisposable) are excluded from automatic registration.
    /// Defaults to true. Typical usage keeps this true while the scanner filters obvious framework noise.
    /// </summary>
    public bool IncludeNonGenericInterfaces { get; } = includeNonGenericInterfaces;

    /// <summary>
    /// Optional explicit list of interfaces to register this implementation under.
    /// Only used when <see cref="RegisterAs"/> is set to <see cref="RegisterAs.SpecificInterfaces"/>.
    /// Each type must be an interface implemented directly by the annotated class.
    /// </summary>
    public Type[]? SpecificInterfaces { get; init; }
}