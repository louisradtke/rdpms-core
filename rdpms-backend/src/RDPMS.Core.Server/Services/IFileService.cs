using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IFileService : IGenericCollectionService<DataFile>
{
    /// <summary>
    /// Get a link that the client can download the file from.
    /// Let the service decide, which <see cref="FileStorageReference"/> to use.
    /// </summary>
    /// <param name="id">Id of the file to download.</param>
    /// <param name="context">The HttpContext of the current request.</param>
    /// <returns></returns>
    Task<Uri> GetFileDownloadUriAsync(Guid id, HttpContext context);

    /// <summary>
    /// Get the link, from which the API directs to the actual file (yielded by <see cref="GetFileDownloadUriAsync"/>.
    /// </summary>
    /// <param name="id">Id of the DataFile</param>
    /// <param name="context">HttpContext</param>
    /// <returns>The rooted URI</returns>
    string GetContentApiUri(Guid id, HttpContext context);

    /// <summary>
    /// Get a <see cref="FileStorageReference"/> by its Id.
    /// </summary>
    /// <param name="id">Id to query for.</param>
    /// <returns>Instance of the file.</returns>
    /// <exception cref="InvalidOperationException">If no <see cref="FileStorageReference"/> with
    /// the given Id exists.</exception>
    public Task<FileStorageReference> GetStorageReferenceByIdAsync(Guid id);

    /// <summary>
    /// Get all <see cref="FileStorageReference"/> instances, optionally filtered by storeId and type.
    /// </summary>
    /// <param name="storeId">The id of the store to query for.</param>
    /// <param name="type">The type to query for</param>
    /// <returns>Enumerable of all matching instances</returns>
    public Task<IEnumerable<FileStorageReference>> GetStorageReferencesAsync(
        Guid? storeId = null, StorageType? type = null);
    
    /// <summary>
    /// Request a file upload to S3 and updates the file in the database.
    /// </summary>
    /// <param name="file">Reference to the file</param>
    /// <param name="reference">Explicit reference to the <see cref="S3FileStorageReference"/>.
    /// Otherwise, the service would have to guess, which of the file's storage refs to take. </param>
    /// <param name="dataSetId">Id of the parent dataset</param>
    /// <param name="dataStoreId"></param>
    /// <returns></returns>
    Task<FileUploadTarget> RequestS3FileUploadAsync(
        DataFile file, S3FileStorageReference reference,
        Guid dataSetId, Guid dataStoreId);
}