using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataFileService(IDataFileRepository repo) : ReadonlyGenericCollectionService<DataFile>(repo), IFileService
{
    public Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        return repo.GetFilesInStoreAsync(storeId);
    }

    public Task<FileUploadTarget> RequestFileUploadAsync(DataFile file)
    {
        return Task.FromResult(new FileUploadTarget(new Uri("https://todo.com")));
    }
}