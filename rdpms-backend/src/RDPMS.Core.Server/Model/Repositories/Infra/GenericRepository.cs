using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Model.Repositories.Infra;

public abstract class GenericRepository<T>(RDPMSPersistenceContext ctx, DbSet<T> dbSet) : IGenericRepository<T>
    where T : class, IUniqueEntity 
{
    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await dbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var entity = await dbSet.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException();
        return entity;
    }

    public async Task<bool> CheckForIdAsync(Guid id)
    {
        // todo: this is dirty, idk
        var any = (await dbSet.ToListAsync()).Any(t => t.Id == id);
        return any;
    }

    public async Task AddAsync(T item)
    {
        await dbSet.AddAsync(item);
        await ctx.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> items)
    {
        await dbSet.AddRangeAsync(items);
        await ctx.SaveChangesAsync();
    }
}