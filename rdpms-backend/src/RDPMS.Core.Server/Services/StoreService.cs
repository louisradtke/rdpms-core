using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class StoreService(IGenericRepository<DataStore> repo) : ReadonlyGenericCollectionService<DataStore>(repo), IStoreService
{
    public Task AddAsync(DataStore item)
    {
        return repo.AddAsync(item);
    }
    
}