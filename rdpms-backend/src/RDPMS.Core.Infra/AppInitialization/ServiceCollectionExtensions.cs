using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RDPMS.Core.Infra.AppInitialization;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register classes marked
/// with <see cref="AutoRegisterAttribute"/>.
/// </summary>
public static class ServiceCollectionAutoRegistrar
{
    /// <summary>
    /// Registers all classes marked with <see cref="AutoRegisterAttribute"/> in the given assemblies.
    /// </summary>
    /// <param name="services">The service collection to register the services at.</param>
    /// <param name="assemblies">e.g. array of dependencies. Use e.g.
    /// <code>new[] { typeof(Program).Assembly };</code></param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddAttributedServices(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies)
    {
        var types = assemblies
            .SelectMany(SafeGetTypes)
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<AutoRegisterAttribute>()))
            .Where(x => x.Attr is not null)
            .ToArray();

        foreach (var (implType, attr) in types)
        {
            var lifetime = attr!.Lifetime;

            if ((attr.RegisterFlags | RegisterFlags.Self) > 0)
            {
                // register as self
                Add(services, implType, implType, lifetime);
            }
            
            var interfaces = new List<Type>();
            if ((attr.RegisterFlags | RegisterFlags.ShallowInterfaces) > 0)
            {
                interfaces.AddRange(GetDirectInterfaces(implType));
            }
            
            interfaces.AddRange(attr.SpecificInterfaces ?? []);

            foreach (var serviceType in interfaces)
            {
                if (ShouldSkip(serviceType, attr)) continue;

                if (serviceType is { IsGenericType: true, ContainsGenericParameters: true })
                {
                    // // Open generic service interface: register as open generic if impl is open
                    // if (implType.IsGenericTypeDefinition && attr.IncludeOpenGenerics)
                    // {
                    //     AddOpenGeneric(services, serviceType.GetGenericTypeDefinition(), implType, lifetime);
                    // }
                    continue;
                }

                // Closed interface
                Add(services, serviceType, implType, lifetime);
            }
        }

        return services;
    }

    private static bool ShouldSkip(Type serviceType, AutoRegisterAttribute attr)
    {
        if (!attr.IncludeNonGenericInterfaces && !serviceType.IsGenericType)
            return true;

        if (serviceType == typeof(IDisposable) || serviceType == typeof(IAsyncDisposable))
            return true;

        if (serviceType.IsGenericType)
        {
            var def = serviceType.GetGenericTypeDefinition();
            if (def == typeof(IEquatable<>) || def == typeof(IComparable<>) || def == typeof(IComparable<>))
                return true;
        }
        return false;
    }

    private static IEnumerable<Type> GetDirectInterfaces(Type type)
    {
        var all = type.GetInterfaces();
        var baseIfs = type.BaseType?.GetInterfaces() ?? Array.Empty<Type>();
        return all.Except(baseIfs);
    }

    private static void Add(IServiceCollection s, Type service, Type impl, ServiceLifetime lifetime)
    {
        var d = new ServiceDescriptor(service, impl, lifetime);
        s.Add(d);
    }

    private static void AddOpenGeneric(IServiceCollection s, Type serviceOpenGeneric, Type implOpenGeneric, ServiceLifetime lifetime)
    {
        var d = new ServiceDescriptor(serviceOpenGeneric, implOpenGeneric, lifetime);
        s.Add(d);
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly a)
    {
        try { return a.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}