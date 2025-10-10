using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Model.Repositories.Infra;

public abstract class GenericRepository<T>(
    DbContext ctx,
    DbSet<T> dbSet,
    IIncludeConfiguration<T>? includeConfiguration) : IGenericRepository<T>
    where T : class, IUniqueEntity
{
    
    protected readonly DbContext Context = ctx;
    protected readonly DbSet<T> DbSet = dbSet;

    protected GenericRepository(DbContext ctx) : this(ctx, ctx.Set<T>(), null)
    {
    }

    protected GenericRepository(DbContext ctx, IIncludeConfiguration<T> configuration)
        : this(ctx, ctx.Set<T>(), configuration)
    {
    }

    protected GenericRepository(DbContext ctx, Func<IQueryable<T>, IQueryable<T>> includeFunc)
        : this(ctx, ctx.Set<T>(), GenericLambdaIncludeConfiguration<T>.Create(includeFunc))
    {
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = DbSet.AsQueryable();
        
        if (includeConfiguration != null)
        {
            query = includeConfiguration.ConfigureIncludes(query);
        }

        return await query
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var query = DbSet.AsQueryable();
        
        if (includeConfiguration != null)
        {
            query = includeConfiguration.ConfigureIncludes(query);
        }
        
        var entity = await query
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
        if (entity == null) throw new KeyNotFoundException();
        return entity;
    }

    public async Task<bool> CheckForIdAsync(Guid id)
    {
        // todo: this is dirty, idk
        var list = await DbSet
            .AsNoTracking()
            .ToListAsync();
        var any = list.Any(t => t.Id == id);
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