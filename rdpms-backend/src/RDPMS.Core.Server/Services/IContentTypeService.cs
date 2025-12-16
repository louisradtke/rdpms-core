using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IContentTypeService : IGenericCollectionService<ContentType>
{
    public Task<ContentType> GetByMimeType(string mimeType, Guid? scopeProject);
}