namespace RDPMS.Core.Persistence.MetadataProjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CachedMetadataProjectionAttribute(
    string id,
    string sourceKey,
    Type strategyType) : Attribute
{
    public string Id { get; } = id;
    public string SourceKey { get; } = sourceKey.ToLowerInvariant();
    public Type StrategyType { get; } = strategyType;

    public string? RequiredSchemaId { get; set; }
    public MetadataSourceScope SourceScope { get; set; } = MetadataSourceScope.Self;

    public string[] OutputNames { get; set; } = [];
    public string[] OutputProperties { get; set; } = [];

    public string? RefreshedAtProperty { get; set; }
    public string? SourceStampProperty { get; set; }
    public string? SourceMetadataFieldIdProperty { get; set; }
    public string? VersionProperty { get; set; }
    public string? ErrorProperty { get; set; }
}
