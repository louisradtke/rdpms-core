using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IStoreService : IReadonlyGenericCollectionService<DataStore>
{
    public Task AddAsync(DataStore item);
}