using System.Reflection;

namespace RDPMS.Core.Persistence.MetadataProjection;

public sealed class MetadataProjectionWriter(CachedMetadataProjectionDescriptor descriptor)
{
    public CachedMetadataProjectionDescriptor Descriptor { get; } = descriptor;

    public void SetOutput(object entity, string outputName, object? value)
    {
        if (!Descriptor.Outputs.TryGetValue(outputName, out var property))
        {
            throw new InvalidOperationException(
                $"Projection '{Descriptor.Id}' does not declare output '{outputName}'.");
        }

        SetValue(entity, property, value);
    }

    public void SetRefreshedAt(object entity, DateTime? value)
    {
        SetStateValue(entity, Descriptor.StateProperties.RefreshedAt, value);
    }

    public void SetSourceStamp(object entity, DateTime? value)
    {
        SetStateValue(entity, Descriptor.StateProperties.SourceStamp, value);
    }

    public void SetSourceMetadataFieldId(object entity, Guid? value)
    {
        SetStateValue(entity, Descriptor.StateProperties.SourceMetadataFieldId, value);
    }

    public void SetVersion(object entity, string? value)
    {
        SetStateValue(entity, Descriptor.StateProperties.Version, value);
    }

    public void SetError(object entity, string? value)
    {
        SetStateValue(entity, Descriptor.StateProperties.Error, value);
    }

    private static void SetStateValue(object entity, PropertyInfo? property, object? value)
    {
        if (property is not null)
        {
            SetValue(entity, property, value);
        }
    }

    private static void SetValue(object entity, PropertyInfo property, object? value)
    {
        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (value is not null && !targetType.IsInstanceOfType(value))
        {
            value = Convert.ChangeType(value, targetType);
        }

        property.SetValue(entity, value);
    }
}
