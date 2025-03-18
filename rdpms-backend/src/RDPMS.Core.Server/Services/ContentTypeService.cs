using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;

namespace RDPMS.Core.Server.Services;

public class ContentTypeService(ContentTypeRepository repo) : IContentTypeService
{
    public Task<IEnumerable<ContentTypeEntity>> GetAllAsync()
    {
        return repo.GetAllAsync();
    }
    
    public Task<ContentTypeEntity> GetContentTypeByGuidAsync(Guid id)
    {
        return repo.GetByIdAsync(id);
    }

    public Task<bool> CheckForIdAsync(Guid id)
    {
        return repo.CheckForIdAsync(id);
    }

    public Task AddAsync(ContentTypeEntity contentType)
    {
        return repo.AddAsync(contentType);
    }

    public Task AddRangeAsync(IEnumerable<ContentTypeEntity> contentType)
    {
        return repo.AddRangeAsync(contentType);
    }
}