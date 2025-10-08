using RDPMS.Core.Infra;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class ProjectService(IProjectRepository projectRepo)
    : GenericCollectionService<Project>(projectRepo), IProjectService
{
    public Task<Project> GetGlobalProjectAsync()
    {
        return projectRepo.GetByIdAsync(RDPMSConstants.GlobalProjectId);
    }

    public async Task UpdateNameAsync(Guid id, string name)
    {
        var existing = await projectRepo.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"DataSet {id} not found");

        existing.Name = name;
        
        await projectRepo.UpdateAsync(existing);
    }
}