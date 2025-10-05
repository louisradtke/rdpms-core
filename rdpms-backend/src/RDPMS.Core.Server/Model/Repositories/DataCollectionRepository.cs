using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataCollectionRepository : GenericRepository<DataCollectionEntity>, IDataCollectionRepository
{
    public DataCollectionRepository(RDPMSPersistenceContext ctx)
        : base(ctx, q => q.Include(e => e.DefaultDataStore))
    {
    }


    public async Task<IEnumerable<DataCollectionEntity>> GetAllInProject(Guid projectId)
    {
        return await DbSet.Where(c => c.ParentId == projectId).ToListAsync();
    }
}