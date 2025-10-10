using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class ProjectService(DbContext context)
    : GenericCollectionService<Project>(context), IProjectService
{
    public Task<Project> GetGlobalProjectAsync()
    {
        return GetByIdAsync(RDPMSConstants.GlobalProjectId);
    }

    public async Task UpdateNameAsync(Guid id, string name)
    {
        var existing = await GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"DataSet {id} not found");

        existing.Name = name;
        
        await UpdateAsync(existing);
    }
}