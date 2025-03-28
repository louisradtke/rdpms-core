using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Services;

public interface IContentTypeService
{
    public Task<IEnumerable<ContentType>> GetAllAsync();
    Task<ContentType> GetByIdAsync(Guid id);
    Task<bool> CheckForIdAsync(Guid id);
    Task AddAsync(ContentType item);
    Task AddRangeAsync(IEnumerable<ContentType> items);
}