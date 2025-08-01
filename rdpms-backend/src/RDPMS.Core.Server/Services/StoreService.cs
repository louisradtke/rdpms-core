using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class StoreService(IDataStoreRepository repo) : ReadonlyGenericCollectionService<DataStore>(repo), IStoreService
{
    public Task AddAsync(DataStore item)
    {
        return repo.AddAsync(item);
    }
    
}