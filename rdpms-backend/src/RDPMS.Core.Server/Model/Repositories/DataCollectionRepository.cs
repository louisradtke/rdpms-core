using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataCollectionRepository : GenericRepository<DataCollectionEntity>, IDataCollectionRepository
{
    public DataCollectionRepository(RDPMSPersistenceContext ctx) : base(ctx, ctx.DataCollections)
    {
    }
}