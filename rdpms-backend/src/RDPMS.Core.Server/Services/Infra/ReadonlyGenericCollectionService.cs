using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Services.Infra;

public abstract class ReadonlyGenericCollectionService<T>(IGenericRepository<T> repo)
    : IReadonlyGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    public Task<IEnumerable<T>> GetAllAsync()
    {
        return repo.GetAllAsync();
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return repo.GetByIdAsync(id);
    }

    public Task<bool> CheckForIdAsync(Guid id)
    {
        return repo.CheckForIdAsync(id);
    }
}