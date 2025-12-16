using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public abstract class ReadonlyGenericCollectionService<T> : IReadonlyGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    protected ReadonlyGenericCollectionService(DbContext context)
    {
        Context = context;
        IncludeConfiguration = null;
    }

    protected ReadonlyGenericCollectionService(DbContext context, IIncludeConfiguration<T> configuration)
    {
        Context = context;
        IncludeConfiguration = configuration;
    }

    protected DbContext Context { get; }
    protected IIncludeConfiguration<T>? IncludeConfiguration { get; }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var query = Context.Set<T>()
            .AsQueryable();
        if (IncludeConfiguration != null)
        {
            query = IncludeConfiguration.ConfigureIncludes(query);
        }
        return await query.ToListAsync();
    }

    public IQueryable<T> Query()
    {
        var query = Context.Set<T>()
            .AsQueryable();
        if (IncludeConfiguration != null)
        {
            query = IncludeConfiguration.ConfigureIncludes(query);
        }

        return query;
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        var query = Context.Set<T>()
            .AsQueryable();
        if (IncludeConfiguration != null)
        {
            query = IncludeConfiguration.ConfigureIncludes(query);
        }
        return await query.SingleAsync(e => e.Id == id);
    }

    public async Task<bool> CheckForIdAsync(Guid id)
    {
        var query = Context.Set<T>()
            .AsQueryable();
        if (IncludeConfiguration != null)
        {
            query = IncludeConfiguration.ConfigureIncludes(query);
        }
        return await query.AnyAsync(e => e.Id == id);
    }
}