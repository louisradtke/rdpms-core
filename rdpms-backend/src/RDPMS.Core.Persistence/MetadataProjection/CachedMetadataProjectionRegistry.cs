using System.Reflection;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence.MetadataProjection;

public sealed class CachedMetadataProjectionRegistry
{
    private readonly List<CachedMetadataProjectionDescriptor> _descriptors;

    public CachedMetadataProjectionRegistry()
        : this([typeof(DataSet).Assembly])
    {
    }

    public CachedMetadataProjectionRegistry(IEnumerable<Assembly> assemblies)
    {
        _descriptors = assemblies
            .SelectMany(SafeGetTypes)
            .SelectMany(type => type
                .GetCustomAttributes<CachedMetadataProjectionAttribute>()
                .Select(attribute => CachedMetadataProjectionDescriptor.Create(type, attribute)))
            .ToList();
    }

    public IReadOnlyCollection<CachedMetadataProjectionDescriptor> GetForEntity(Type entityType, string sourceKey)
    {
        var normalizedKey = sourceKey.ToLowerInvariant();
        return _descriptors
            .Where(d => d.EntityType.IsAssignableFrom(entityType))
            .Where(d => d.SourceKey == normalizedKey)
            .ToList();
    }

    public IReadOnlyCollection<CachedMetadataProjectionDescriptor> GetAll()
    {
        return _descriptors;
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
