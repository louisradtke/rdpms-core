using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class StoreService(
    DbContext context,
    IS3Service s3Service
)
    : GenericCollectionService<DataStore>(context), IStoreService
{
    public async Task<FileUploadTarget> RequestS3FileUploadAsync(
        DataFile file, S3FileStorageReference reference,
        Guid dataSetId, Guid dataStoreId)
    {
        var store = await Context.Set<DataStore>()
            .SingleOrDefaultAsync(s => s.Id == dataStoreId && s.StorageType == StorageType.S3)
            as S3DataStore;
        if (store is null)
        {
            throw new InvalidOperationException("Store not found or not S3!");
        }
        
        var dataSetExits = await Context.Set<DataSet>()
            .AnyAsync(d => d.Id == dataSetId && d.State == DataSetState.Uninitialized);
        if (!dataSetExits)
        {
            throw new InvalidOperationException("DataSet not found or not in invalid state!");
        }

        var key = $"{dataSetId}/{file.Name}";
        var uploadUrl = await s3Service.RequestPresignedUploadUrlAsync(store, key);

        reference.ObjectKey = key;

        return new FileUploadTarget(new Uri(uploadUrl))
        {
            FileId = file.Id
        };
    }
}