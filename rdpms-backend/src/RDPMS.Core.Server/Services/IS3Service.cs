using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public interface IS3Service
{
    Task<string> RequestPresignedUploadUrlAsync(S3DataStore store, string key);
    Task<string> RequestPresignedDownloadUrlAsync(S3FileStorageReference reference, S3DataStore store);
    Task<IEnumerable<string>> ListAllFilesAsync(S3DataStore store);
}