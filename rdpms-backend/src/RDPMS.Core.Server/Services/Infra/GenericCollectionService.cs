using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public class GenericCollectionService<T>(IGenericRepository<T> repo)
    : ReadonlyGenericCollectionService<T>(repo), IGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    private readonly IGenericRepository<T> _repository = repo;

    public Task AddAsync(T item)
    {
        return _repository.AddAsync(item);
    }
    
    public Task AddRangeAsync(IEnumerable<T> items)
    {
        return _repository.AddRangeAsync(items);
    }
}