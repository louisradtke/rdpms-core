using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataSetRepository(DbContext ctx) : GenericRepository<DataSet>(ctx,
        files => files
            .Include(ds => ds.Files)
            .ThenInclude(f => f.FileType)
            .Include(ds => ds.Files)
            .ThenInclude(f => f.Locations)
        ),
    IDataSetRepository
{
    public async Task<IEnumerable<DataSet>> GetByCollectionIdAsync(Guid collectionId)
    {
        var collection = await Context.Set<DataCollectionEntity>()
            .AsNoTracking()
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.Files)
            .ThenInclude(f => f.FileType)
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.Files)
            .ThenInclude(f => f.Locations)
            .SingleAsync(e => e.Id == collectionId);
        return collection.ContainedDatasets;
    }
    
    public async Task UpdateFieldsAsync(DataSet entity)
    {
        Context.Set<DataSet>().Update(entity);
        await ctx.SaveChangesAsync();
    }
}