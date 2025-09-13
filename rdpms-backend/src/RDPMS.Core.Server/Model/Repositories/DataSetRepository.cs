using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataSetRepository : GenericRepository<DataSet>,
        IDataSetRepository
{
    private readonly DbSet<DataCollectionEntity> _collectionsDbSet;

    public DataSetRepository(RDPMSPersistenceContext ctx) : base(ctx)
    {
        _collectionsDbSet = ctx.DataCollections;
    }

    public async Task<IEnumerable<DataSet>> GetByCollectionIdAsync(Guid collectionId)
    {
        var collection = await _collectionsDbSet
            .Include(c => c.ContainedDatasets)
            .SingleAsync(e => e.Id == collectionId);
        return collection.ContainedDatasets;
    }
}