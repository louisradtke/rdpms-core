using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IFileService : IReadonlyGenericCollectionService<DataFile>
{
    Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId);
    Task<FileUploadTarget> RequestFileUploadAsync(DataFile file);
}