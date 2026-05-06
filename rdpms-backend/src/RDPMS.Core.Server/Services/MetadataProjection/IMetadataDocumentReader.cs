using System.Text.Json;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services.MetadataProjection;

public interface IMetadataDocumentReader
{
    Task<JsonDocument> ReadJsonDocumentAsync(
        MetadataJsonField metadataField,
        CancellationToken cancellationToken = default);
}
