using System.Reflection;
using System.Text.Json;

namespace RDPMS.Core.Contracts;

public interface ISchemaRepository
{
    IReadOnlyCollection<SchemaDescriptor> GetAll();
    bool TryGetById(Guid id, out SchemaDescriptor descriptor);
    Stream OpenSchemaStream(Guid id);
    string GetSchemaString(Guid id);
}

public sealed record SchemaDescriptor(
    Guid Id,
    string SchemaPath,
    Type GeneratedType
);

public sealed class EmbeddedSchemaRepository : ISchemaRepository
{
    private const string IndexResourceSuffix = "Schemas.index.json";

    private readonly Assembly _assembly;
    private readonly Dictionary<Guid, SchemaDescriptor> _schemasById;
    private readonly Dictionary<Guid, string> _resourceNameById;

    public EmbeddedSchemaRepository()
        : this(typeof(EmbeddedSchemaRepository).Assembly)
    {
    }

    internal EmbeddedSchemaRepository(Assembly assembly)
    {
        _assembly = assembly;
        (_schemasById, _resourceNameById) = LoadIndexAndResources();
    }

    public IReadOnlyCollection<SchemaDescriptor> GetAll()
    {
        return _schemasById.Values.ToList().AsReadOnly();
    }

    public bool TryGetById(Guid id, out SchemaDescriptor descriptor)
    {
        return _schemasById.TryGetValue(id, out descriptor!);
    }

    public Stream OpenSchemaStream(Guid id)
    {
        var resourceName = GetResourceName(id);
        return _assembly.GetManifestResourceStream(resourceName)
               ?? throw new InvalidOperationException($"Schema resource '{resourceName}' was not found.");
    }

    public string GetSchemaString(Guid id)
    {
        using var stream = OpenSchemaStream(id);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private string GetResourceName(Guid id)
    {
        if (_resourceNameById.TryGetValue(id, out var resourceName))
        {
            return resourceName;
        }

        throw new KeyNotFoundException($"Schema with id '{id}' was not registered.");
    }

    private (Dictionary<Guid, SchemaDescriptor> schemasById, Dictionary<Guid, string> resourceNameById)
        LoadIndexAndResources()
    {
        var allResourceNames = _assembly.GetManifestResourceNames();
        var indexResourceName = allResourceNames.SingleOrDefault(
            n => n.EndsWith(IndexResourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (indexResourceName is null)
        {
            throw new InvalidOperationException($"Could not find embedded schema index resource '*.{IndexResourceSuffix}'.");
        }

        using var indexStream = _assembly.GetManifestResourceStream(indexResourceName)
                                ?? throw new InvalidOperationException(
                                    $"Could not open embedded schema index resource '{indexResourceName}'.");
        using var reader = new StreamReader(indexStream);
        var json = reader.ReadToEnd();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var entries = JsonSerializer.Deserialize<List<SchemaIndexEntry>>(json, options)
                                ?? throw new InvalidOperationException("Failed to deserialize schema index.");

        var descriptors = new Dictionary<Guid, SchemaDescriptor>();
        var resourceNames = new Dictionary<Guid, string>();

        foreach (var entry in entries)
        {
            if (entry.Id == Guid.Empty)
            {
                throw new InvalidOperationException($"Invalid schema id in index for '{entry.Schema}'.");
            }

            if (string.IsNullOrWhiteSpace(entry.Schema))
            {
                throw new InvalidOperationException("Schema path in index must not be empty.");
            }

            if (string.IsNullOrWhiteSpace(entry.ClassName))
            {
                throw new InvalidOperationException($"Class name in index must not be empty for '{entry.Schema}'.");
            }

            var generatedType = _assembly.GetType(entry.ClassName, throwOnError: false, ignoreCase: false)
                                ?? throw new InvalidOperationException(
                                    $"Generated type '{entry.ClassName}' for schema '{entry.Schema}' was not found.");

            var resourceName = ResolveSchemaResourceName(entry.Schema, allResourceNames);
            var descriptor = new SchemaDescriptor(entry.Id, entry.Schema, generatedType);

            descriptors.Add(entry.Id, descriptor);
            resourceNames.Add(entry.Id, resourceName);
        }

        return (descriptors, resourceNames);
    }

    private static string ResolveSchemaResourceName(string schemaPath, IEnumerable<string> allResourceNames)
    {
        var normalizedPath = schemaPath.Replace('\\', '/');
        var resourceSuffix = $"Schemas.{normalizedPath.Replace('/', '.')}";
        var resourceName = allResourceNames.SingleOrDefault(
            n => n.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));

        return resourceName
               ?? throw new InvalidOperationException(
                   $"Embedded resource for schema '{schemaPath}' (suffix '{resourceSuffix}') was not found.");
    }

    private sealed record SchemaIndexEntry
    {
        public string Schema { get; init; } = string.Empty;
        public Guid Id { get; init; }
        public string ClassName { get; init; } = string.Empty;
    }
}
