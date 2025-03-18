using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class ContentTypeRepository : GenericRepository<ContentType>
{
    public ContentTypeRepository(RDPMSPersistenceContext ctx) : base(ctx, ctx.Types)
    {
    }
}