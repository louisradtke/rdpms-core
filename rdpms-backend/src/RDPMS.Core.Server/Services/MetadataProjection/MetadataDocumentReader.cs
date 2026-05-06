using System.Text.Json;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Services.MetadataProjection;

public sealed class MetadataDocumentReader(
    IStoreService storeService,
    IS3Service s3Service)
    : IMetadataDocumentReader
{
    public async Task<JsonDocument> ReadJsonDocumentAsync(
        MetadataJsonField metadataField,
        CancellationToken cancellationToken = default)
    {
        var value = metadataField.Value ??
                    throw new InvalidOperationException("Metadata field has no backing data file.");

        var dbReference = value.References.OfType<DbFileStorageReference>().FirstOrDefault();
        if (dbReference is not null)
        {
            return JsonDocument.Parse(dbReference.Data);
        }

        var s3Reference = value.References.OfType<S3FileStorageReference>().FirstOrDefault();
        if (s3Reference is not null)
        {
            if (s3Reference.StoreFid is null)
            {
                throw new InvalidOperationException("S3 metadata reference has no data store id.");
            }

            var store = await storeService.GetByIdAsync(s3Reference.StoreFid.Value) as S3DataStore
                        ?? throw new InvalidOperationException("S3 metadata reference points to a non-S3 data store.");
            var bytes = await s3Service.GetFileAsync(s3Reference, store);
            cancellationToken.ThrowIfCancellationRequested();
            return JsonDocument.Parse(bytes);
        }

        throw new InvalidOperationException("Metadata field has no readable JSON storage reference.");
    }
}
