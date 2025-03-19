using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataSetRepository : GenericRepository<DataSet>
{
    public DataSetRepository(RDPMSPersistenceContext ctx) : base(ctx, ctx.DataSets)
    {
    }
}