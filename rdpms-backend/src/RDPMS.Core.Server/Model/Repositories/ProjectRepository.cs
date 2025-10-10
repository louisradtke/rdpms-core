using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class ProjectRepository(DbContext ctx)
    : GenericRepository<Project>(ctx, q => q
        .Include(p => p.DataCollections)
        .Include(p => p.DataStores)
    ), IProjectRepository
{
    public async Task UpdateAsync(Project entity)
    {
        DbSet.Update(entity);
        await Context.SaveChangesAsync();
    }
}