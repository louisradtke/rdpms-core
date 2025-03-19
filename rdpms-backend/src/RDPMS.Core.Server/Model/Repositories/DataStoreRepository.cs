using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataStoreRepository : GenericRepository<DataStore>
{
    public DataStoreRepository(RDPMSPersistenceContext ctx) : base(ctx, ctx.DataStores)
    {
    }
}