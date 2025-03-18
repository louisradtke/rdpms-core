namespace RDPMS.Core.Server.Model.Repositories.Infra;

public interface IGenericRepository<T> where T : class
{
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<T> GetByIdAsync(Guid id);
    public Task<bool> CheckForIdAsync(Guid id);
    public Task AddAsync(T item);
    public Task AddRangeAsync(IEnumerable<T> items);
}