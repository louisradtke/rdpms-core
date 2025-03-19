using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataContainerRepository : GenericRepository<DataContainer>
{
    public DataContainerRepository(RDPMSPersistenceContext ctx) : base(ctx, ctx.DataContainers)
    {
    }
}