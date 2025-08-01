using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class ContentTypeService : GenericCollectionService<ContentType>, IContentTypeService
{
    public ContentTypeService(IContentTypeRepository repo) : base(repo)
    {
    }

}