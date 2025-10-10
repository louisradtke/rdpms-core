using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IProjectService : IGenericCollectionService<Project>
{
    Task<Project> GetGlobalProjectAsync();
    public Task UpdateNameAndSlugAsync(Guid id, string name, string? slug);
}