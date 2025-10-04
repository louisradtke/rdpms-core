using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataSetRepository(RDPMSPersistenceContext ctx) : GenericRepository<DataSet>(ctx,
        files => files
            .Include(ds => ds.Files)
            .ThenInclude(f => f.FileType)
            .Include(ds => ds.Files)
            .ThenInclude(f => f.Locations)
        ),
    IDataSetRepository
{
    private readonly DbSet<DataCollectionEntity> _collectionsDbSet = ctx.DataCollections;

    public async Task<IEnumerable<DataSet>> GetByCollectionIdAsync(Guid collectionId)
    {
        var collection = await _collectionsDbSet
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.Files)
            .ThenInclude(f => f.FileType)
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.Files)
            .ThenInclude(f => f.Locations)
            .SingleAsync(e => e.Id == collectionId);
        return collection.ContainedDatasets;
    }
}