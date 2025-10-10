using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public class GenericCollectionService<T>(DbContext context)
    : ReadonlyGenericCollectionService<T>(context), IGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    public async Task AddAsync(T item)
    {
        await Context.Set<T>()
            .AddAsync(item);
        await Context.SaveChangesAsync();
    }
    
    public Task AddRangeAsync(IEnumerable<T> items)
    {
        Context.Set<T>()
            .AddRangeAsync(items);
        return Context.SaveChangesAsync();
    }

    public Task UpdateAsync(T item)
    {
        Context.Update(item);
        return Context.SaveChangesAsync();
    }
}