using System.Reflection;

namespace RDPMS.Core.Persistence.MetadataProjection;

public sealed class CachedMetadataProjectionDescriptor
{
    private CachedMetadataProjectionDescriptor(
        string id,
        Type entityType,
        string sourceKey,
        Type strategyType,
        string? requiredSchemaId,
        MetadataSourceScope sourceScope,
        IReadOnlyDictionary<string, PropertyInfo> outputs,
        ProjectionStateProperties stateProperties)
    {
        Id = id;
        EntityType = entityType;
        SourceKey = sourceKey;
        StrategyType = strategyType;
        RequiredSchemaId = requiredSchemaId;
        SourceScope = sourceScope;
        Outputs = outputs;
        StateProperties = stateProperties;
    }

    public string Id { get; }
    public Type EntityType { get; }
    public string SourceKey { get; }
    public Type StrategyType { get; }
    public string? RequiredSchemaId { get; }
    public MetadataSourceScope SourceScope { get; }
    public IReadOnlyDictionary<string, PropertyInfo> Outputs { get; }
    public ProjectionStateProperties StateProperties { get; }

    public static CachedMetadataProjectionDescriptor Create(
        Type entityType,
        CachedMetadataProjectionAttribute attribute)
    {
        if (!typeof(IMetadataProjectionStrategy).IsAssignableFrom(attribute.StrategyType))
        {
            throw new InvalidOperationException(
                $"Projection strategy '{attribute.StrategyType.FullName}' must implement {nameof(IMetadataProjectionStrategy)}.");
        }

        if (attribute.OutputNames.Length != attribute.OutputProperties.Length)
        {
            throw new InvalidOperationException(
                $"Projection '{attribute.Id}' on '{entityType.FullName}' has mismatched output names and properties.");
        }

        var outputs = attribute.OutputNames
            .Zip(attribute.OutputProperties)
            .ToDictionary(
                pair => pair.First,
                pair => GetWritableProperty(entityType, pair.Second));

        var stateProperties = new ProjectionStateProperties(
            GetOptionalWritableProperty(entityType, attribute.RefreshedAtProperty),
            GetOptionalWritableProperty(entityType, attribute.SourceStampProperty),
            GetOptionalWritableProperty(entityType, attribute.SourceMetadataFieldIdProperty),
            GetOptionalWritableProperty(entityType, attribute.VersionProperty),
            GetOptionalWritableProperty(entityType, attribute.ErrorProperty));

        return new CachedMetadataProjectionDescriptor(
            attribute.Id,
            entityType,
            attribute.SourceKey,
            attribute.StrategyType,
            attribute.RequiredSchemaId,
            attribute.SourceScope,
            outputs,
            stateProperties);
    }

    private static PropertyInfo GetWritableProperty(Type entityType, string propertyName)
    {
        var property = entityType.GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null || property.SetMethod is null)
        {
            throw new InvalidOperationException(
                $"Projection property '{propertyName}' was not found or is not writable on '{entityType.FullName}'.");
        }

        return property;
    }

    private static PropertyInfo? GetOptionalWritableProperty(Type entityType, string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
            ? null
            : GetWritableProperty(entityType, propertyName);
    }
}

public sealed record ProjectionStateProperties(
    PropertyInfo? RefreshedAt,
    PropertyInfo? SourceStamp,
    PropertyInfo? SourceMetadataFieldId,
    PropertyInfo? Version,
    PropertyInfo? Error);
