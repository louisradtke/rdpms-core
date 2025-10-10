using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Controllers.V1;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataFileService(DbContext dbContext, LinkGenerator linkGenerator)
    : ReadonlyGenericCollectionService<DataFile>(dbContext), IFileService
{
    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        var store = await Context.Set<DataStore>()
            .Include(s => s.DataFiles)
            .ThenInclude(f => f.Locations)
            .SingleAsync(s => s.Id == storeId);

        return store.DataFiles;
    }

    public Task<FileUploadTarget> RequestFileUploadAsync(DataFile file)
    {
        return Task.FromResult(new FileUploadTarget(new Uri("https://todo.com")));
    }

    public async Task<Uri> GetFileDownloadUriAsync(Guid id, HttpContext context)
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
            // case InternalFileStorageReference:
            //     var uri = linkGenerator.GetUriByAction(context, nameof(FilesController.GetContent));
            //     return new Uri(uri);
            default:
                throw new IllegalStateException("Invalid file location type");
        }
    }

    public string GetContentApiUri(Guid id, HttpContext context)
    {
        var uri = linkGenerator.GetUriByAction(
            context, nameof(FilesController.GetContent), "Files",
            new { id });
        return uri ?? throw new IllegalStateException("Invalid file location type");
    }
}