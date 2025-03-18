using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Services;

public interface IFileService
{
    Task<IEnumerable<DataFile>> GetFilesAsync();
    Task<DataFile> GetFileAsync(Guid id);
    Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId);
    Task<FileUploadTarget> RequestFileUploadAsync(DataFile file);
}