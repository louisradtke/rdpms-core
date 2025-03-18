using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Services;

public interface IContentTypeService
{
    public Task<IEnumerable<ContentTypeEntity>> GetAllAsync();
    Task<ContentTypeEntity> GetContentTypeByGuidAsync(Guid id);
    Task<bool> CheckForIdAsync(Guid id);
    Task AddAsync(ContentTypeEntity contentType);
    Task AddRangeAsync(IEnumerable<ContentTypeEntity> contentType);
}