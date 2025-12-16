using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Services;

public class ContentTypeService(DbContext dbContext)
    : GenericCollectionService<ContentType>(dbContext), IContentTypeService
{
    public async Task<ContentType> GetByMimeType(string mimeType, Guid? scopeProject)
    {
        var projects = new List<Guid> {RDPMSConstants.GlobalProjectId};
        if (scopeProject.HasValue) projects.Add(scopeProject.Value);

        var ret = await Query()
            .Where(c => projects.Contains(c.ParentProjectId))
            .SingleOrDefaultAsync(c => c.MimeType == mimeType);
        return ret ?? throw new IllegalStateException("No default JSON content type found!");
    }
}