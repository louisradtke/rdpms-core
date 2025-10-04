using Microsoft.Extensions.DependencyInjection;

namespace RDPMS.Core.Infra.AppInitialization;


/// <summary>
/// Marks a concrete class for automatic dependency injection registration during startup.
/// </summary>
/// <param name="lifetime">How to register the container, see
/// https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes</param>
/// <param name="registerFlags">flags (bitwise) for which types to register. See docs of <see cref="RegisterFlags"/>
/// for more information</param>
/// <param name="includeNonGenericInterfaces"></param>
/// <remarks>
/// Usage:
/// - Apply this attribute to concrete classes that you want to be auto-registered in the DI container.
/// - By default, the implementation is registered for all of its direct interfaces (shallow).
/// - You can override the lifetime and the registration behavior via the attribute parameters.
/// </remarks>
/// <example>
/// [AutoRegister(ServiceLifetime.Singleton)]
/// public class FileSummaryDTOMapper : IExportMapper&lt;DataFile, FileSummaryDTO&gt; { ... }
/// </example>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class AutoRegisterAttribute(
    ServiceLifetime lifetime = ServiceLifetime.Singleton,
    RegisterFlags registerFlags = RegisterFlags.ShallowInterfaces,
    // bool includeOpenGenerics = true,
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
    /// - <see cref="RegisterFlags.ShallowInterfaces"/>: Register for all direct (shallow) interfaces implemented by the class (default).
    /// - <see cref="RegisterFlags.Self"/>: Register only as its concrete type.
    /// </summary>
    public RegisterFlags RegisterFlags { get; } = registerFlags;

    // /// <summary>
    // /// When true, enables registering open-generic service mappings for open-generic implementations,
    // /// e.g., IExportMapper&lt;,&gt; → GenericMapper&lt;,&gt;.
    // /// Has no effect for closed generic or non-generic types. Defaults to true.
    // /// </summary>
    // public bool IncludeOpenGenerics { get; } = includeOpenGenerics;

    /// <summary>
    /// When false, non-generic interfaces (e.g., IDisposable) are excluded from automatic registration.
    /// Defaults to true. Typical usage keeps this true while the scanner filters obvious framework noise.
    /// </summary>
    public bool IncludeNonGenericInterfaces { get; } = includeNonGenericInterfaces;

    /// <summary>
    /// Optional explicit list of extra interfaces to register this implementation under.
    /// Each type must be an interface implemented directly by the annotated class.
    /// </summary>
    public Type[]? SpecificInterfaces { get; init; }
}