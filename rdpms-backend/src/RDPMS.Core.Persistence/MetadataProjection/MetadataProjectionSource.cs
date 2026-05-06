using System.Text.Json;

namespace RDPMS.Core.Persistence.MetadataProjection;

public sealed record MetadataProjectionSource(
    bool Exists,
    JsonElement? Document,
    Guid? MetadataFieldId,
    DateTime? SourceStamp)
{
    public static MetadataProjectionSource Missing { get; } = new(false, null, null, null);
}
