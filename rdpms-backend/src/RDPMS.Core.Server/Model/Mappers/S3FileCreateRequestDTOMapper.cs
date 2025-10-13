using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class S3FileCreateRequestDTOMapper : IImportMapper<DataFile, S3FileCreateRequestDTO, ContentType>
{
    public DataFile Import(S3FileCreateRequestDTO foreign, ContentType arg)
    {
        if (foreign.Name == null || foreign.SizeBytes == null || foreign.CreatedStamp == null || foreign.PlainSHA256Hash == null)
        {
            throw new ArgumentException(
                "Name, Size, CreatedStamp, and Hash are required fields in DataFileCreateRequestDTO");
        }
        
        if ((foreign.BeginStamp == null) != (foreign.EndStamp == null))
        {
            throw new ArgumentException("BeginStamp and EndStamp must be both null or both non-null.");
        }

        CompressionAlgorithm? compression = null;
        if (foreign.CompressionAlgorithm != null)
        {
            if (!Enum.TryParse<CompressionAlgorithm>(foreign.CompressionAlgorithm, out var result))
            {
                throw new ArgumentException($"Unknown compression algorithm: '{foreign.CompressionAlgorithm}'");
            }
            compression = result;
            if (compression != CompressionAlgorithm.Plain && foreign.CompressedSHA256Hash == null)
            {
                throw new ArgumentException("CompressedSHA256Hash is required when compression is not plain.");
            }
            if (compression != CompressionAlgorithm.Plain && foreign.CompressedSizeBytes == null)
            {
                throw new ArgumentException("CompressedSizeBytes is required when compression is not plain.");
            }
        }

        return new DataFile(name: foreign.Name)
        {
            FileType = arg,
            SizeBytes = foreign.SizeBytes.Value,
            SHA256Hash = foreign.PlainSHA256Hash,
            CreatedStamp = foreign.CreatedStamp.Value,
            BeginStamp = foreign.BeginStamp,
            EndStamp = foreign.EndStamp,
            Locations = [new S3FileStorageReference
            {
                Algorithm = compression ?? CompressionAlgorithm.Plain,
                SHA256Hash = foreign.CompressedSHA256Hash ?? foreign.PlainSHA256Hash,
                SizeBytes = foreign.CompressedSizeBytes ?? foreign.SizeBytes.Value
            }]
        };
    }

    public IEnumerable<CheckSet<S3FileCreateRequestDTO>> ImportChecks() => [];
}