using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Services;

public interface IFileService
{
    Task<IEnumerable<DataFileEntity>> GetFilesAsync();
    Task<DataFileEntity> GetFileAsync(Guid id);
    Task<IEnumerable<DataFileEntity>> GetFilesInStoreAsync(Guid storeId);
    Task<FileUploadTarget> RequestFileUploadAsync(DataFileEntity file);
}