using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public class GenericCollectionService<T>(IGenericRepository<T> repo)
    : ReadonlyGenericCollectionService<T>(repo), IGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    public Task AddAsync(T item)
    {
        return repo.AddAsync(item);
    }
    
    public Task AddRangeAsync(IEnumerable<T> items)
    {
        return repo.AddRangeAsync(items);
    }
}