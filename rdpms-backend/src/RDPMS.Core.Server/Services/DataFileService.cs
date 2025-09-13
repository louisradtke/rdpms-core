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

    public async Task<Uri> GetFileDownloadUriAsync(Guid id)
    {
        var file = await GetByIdAsync(id);
        var location = file.Locations
            .FirstOrDefault(lRef => lRef.StorageType == StorageType.S3);
        location ??= file.Locations.FirstOrDefault();

        switch (location)
        {
            case S3FileStorageReference:
                throw new NotImplementedException("S3 is yet not implemented!");
            case StaticFileStorageReference staticLocation:
                return new Uri(staticLocation.URL);
            default:
                throw new IllegalStateException("Invalid file location type");
        }
    }
}