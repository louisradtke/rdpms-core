using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public interface IS3Service
{

    /// <summary>
    /// Request a presigned URL for uploading a file to an S3 (-compatible store) bucket.
    /// </summary>
    /// <param name="store">The S3 data store representation.</param>
    /// <param name="key">the file key (not etag).</param>
    /// <returns>URL where the file can be uploaded to.</returns>
    Task<string> RequestPresignedUploadUrlAsync(S3DataStore store, string key);

    /// <summary>
    /// Request a presigned URL for downloading a file from an S3 (-compatible store) bucket.
    /// </summary>
    /// <param name="reference">Representation of the file in S3.</param>
    /// <param name="store">The S3 data store representation.</param>
    /// <returns>Presigned URL where the file can be fetched from.</returns>
    Task<string> RequestPresignedDownloadUrlAsync(S3FileStorageReference reference, S3DataStore store);

    /// <summary>
    /// Fetch a file from S3.
    /// </summary>
    /// <param name="reference">Representation of the file in S3.</param>
    /// <param name="store">The S3 data store representation.</param>
    /// <returns>The plain bytes.</returns>
    Task<byte[]> GetFileAsync(S3FileStorageReference reference, S3DataStore store);

    /// <summary>
    /// List of all files in the bucket, matching the prefix of store.
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> ListAllFilesAsync(S3DataStore store);
    
    /// <summary>
    /// Validate whether a file reference matches a stored file.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="store"></param>
    /// <returns></returns>
    Task<bool> ValidateFileRefAsync(S3FileStorageReference reference, S3DataStore store);
}