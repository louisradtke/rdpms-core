using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataCollectionEntityService(IDataCollectionRepository repo)
    : GenericCollectionService<DataCollectionEntity>(repo), IDataCollectionEntityService
{
    public Task<IEnumerable<int>> GetDatasetCounts(IEnumerable<Guid> collectionIds)
    {
        return repo.GetDatasetCounts(collectionIds);
    }
}