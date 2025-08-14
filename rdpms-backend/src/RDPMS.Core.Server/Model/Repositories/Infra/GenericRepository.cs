using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Model.Repositories.Infra;

public abstract class GenericRepository<T>(RDPMSPersistenceContext ctx, DbSet<T> dbSet) : IGenericRepository<T>
    where T : class, IUniqueEntity
{
    
    protected readonly RDPMSPersistenceContext Context = ctx;
    protected readonly DbSet<T> DbSet = dbSet;

    protected GenericRepository(RDPMSPersistenceContext ctx) : this(ctx, ctx.Set<T>())
    {
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await DbSet.ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException();
        return entity;
    }

    public async Task<bool> CheckForIdAsync(Guid id)
    {
        // todo: this is dirty, idk
        var any = (await DbSet.ToListAsync()).Any(t => t.Id == id);
        return any;
    }

    public async Task AddAsync(T item)
    {
        await DbSet.AddAsync(item);
        await Context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> items)
    {
        await DbSet.AddRangeAsync(items);
        await Context.SaveChangesAsync();
    }
}