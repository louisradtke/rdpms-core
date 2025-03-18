using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;

namespace RDPMS.Core.Server.Services;

public class ContentTypeService(ContentTypeRepository repo) : IContentTypeService
{
    public Task<IEnumerable<ContentType>> GetAllAsync()
    {
        return repo.GetAllAsync();
    }
    
    public Task<ContentType> GetContentTypeByGuidAsync(Guid id)
    {
        return repo.GetByIdAsync(id);
    }

    public Task<bool> CheckForIdAsync(Guid id)
    {
        return repo.CheckForIdAsync(id);
    }

    public Task AddAsync(ContentType contentType)
    {
        return repo.AddAsync(contentType);
    }
}