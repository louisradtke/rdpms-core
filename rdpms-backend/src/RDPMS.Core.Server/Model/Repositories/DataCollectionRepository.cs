using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataCollectionRepository(DbContext ctx)
    : GenericRepository<DataCollectionEntity>(ctx, q => q
        .Include(e => e.DefaultDataStore)
    ), IDataCollectionRepository
{
    public async Task<IEnumerable<DataCollectionEntity>> GetAllInProject(Guid projectId)
    {
        return await DbSet.Where(c => c.ParentId == projectId).ToListAsync();
    }

    public async Task<IEnumerable<int>> GetDatasetCounts(IEnumerable<Guid> collectionIds)
    {
        var collectionIdsList = collectionIds.ToList();
    
        var counts = await DbSet
            .AsNoTracking()
            .Where(c => collectionIdsList.Contains(c.Id))
            .Select(c => new { c.Id, Count = c.ContainedDatasets.Count })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
    
        // Return counts in the same order as the input collectionIds
        return collectionIdsList.Select(id => counts.GetValueOrDefault(id, 0));
    }
}