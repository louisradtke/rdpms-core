using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Services.Infra;

public interface IReadonlyGenericCollectionService<T> 
    where T : class, IUniqueEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(Guid id);
    Task<bool> CheckForIdAsync(Guid id);
}