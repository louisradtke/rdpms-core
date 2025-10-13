using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataCollectionEntityService(DbContext dbContext)
    : GenericCollectionService<DataCollectionEntity>(dbContext, q => q
        .Include(c => c.ContainedDatasets)
        .Include(c => c.DefaultDataStore)
    ), IDataCollectionEntityService
{
    public Task<Dictionary<Guid, int>> GetDatasetCounts(IEnumerable<Guid> collectionIds)
    {
        var collectionIdsList = collectionIds.ToList();
    
        return Context.Set<DataCollectionEntity>()
            .Where(c => collectionIdsList.Contains(c.Id))
            .Select(c => new { c.Id, Count = c.ContainedDatasets.Count })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
    }
}