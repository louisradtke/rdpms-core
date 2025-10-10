using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public abstract class ReadonlyGenericCollectionService<T>(DbContext context)
    : IReadonlyGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    protected DbContext Context { get; } = context;

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Context.Set<T>()
            .ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await Context.Set<T>()
            .SingleAsync(e => e.Id == id);
    }

    public async Task<bool> CheckForIdAsync(Guid id)
    {
        return await Context.Set<T>()
            .AnyAsync(e => e.Id == id);
    }
}