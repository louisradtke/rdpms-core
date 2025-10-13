using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IStoreService : IReadonlyGenericCollectionService<DataStore>
{
    Task AddAsync(DataStore item);

    Task<FileUploadTarget> RequestS3FileUploadAsync(
        DataFile file, S3FileStorageReference reference,
        Guid dataSetId, Guid dataStoreId);
}