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

public class DataFileService(
    DbContext dbContext,
    LinkGenerator linkGenerator)
    : GenericCollectionService<DataFile>(dbContext, files => files
        .Include(f => f.Locations)
        .Include(f => f.FileType)
    ), IFileService
{

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

    public Task<FileStorageReference> GetStorageReferenceByIdAsync(Guid id)
    {
        return Context.Set<FileStorageReference>()
            .SingleAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FileStorageReference>> GetStorageReferencesAsync(
        Guid? storeId = null, StorageType? type = null)
    {
        var query = Context.Set<FileStorageReference>()
            .AsQueryable();
        if (storeId is not null)
        {
            query = query.Where(r => r.StoreFid == storeId);
        }
        if (type != null)
        {
            query = query.Where(r => r.StorageType == type);
        }
        return await query.ToListAsync();
    }
}