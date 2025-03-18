using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Services.Infra;

public interface IGenericCollectionService<T> : IReadonlyGenericCollectionService<T>
    where T : class, IUniqueEntity
{
    Task AddAsync(T item);
    Task AddRangeAsync(IEnumerable<T> items);
}