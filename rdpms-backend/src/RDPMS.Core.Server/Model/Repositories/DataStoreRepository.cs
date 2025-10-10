using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataStoreRepository(DbContext ctx)
    : GenericRepository<DataStore>(ctx), IDataStoreRepository
{
    public async Task<IEnumerable<DataStore>> GetAllInProject(Guid projectId)
    {
        return await DbSet
            .AsNoTracking()
            .Where(s => s.ParentId == projectId)
            .ToListAsync();
    }
}