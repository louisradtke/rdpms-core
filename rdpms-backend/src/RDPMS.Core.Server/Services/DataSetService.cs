using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataSetService(
    IDataSetRepository repo) : GenericCollectionService<DataSet>(repo),
    IDataSetService
{
    public Task<IEnumerable<DataSet>> GetByCollectionAsync(Guid collectionId)
    {
        return repo.GetByCollectionIdAsync(collectionId);
    }
}