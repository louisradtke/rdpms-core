using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class ProjectSummaryDTOMapper(
    IExportMapper<DataStore, DataStoreSummaryDTO> dsExportMapper,
    IExportMapper<DataCollectionEntity, CollectionSummaryDTO> dcExportMapper)
    : IExportMapper<Project, ProjectSummaryDTO>,
        IImportMapper<Project, ProjectSummaryDTO>
{
    public ProjectSummaryDTO Export(Project domain)
    {
        return new ProjectSummaryDTO()
        {
            Id = domain.Id,
            Slug = domain.Slug,
            Name = domain.Name,
            Description = domain.Description,
            DataStores = domain.DataStores.Select(dsExportMapper.Export).ToList(),
            Collections = domain.DataCollections.Select(dcExportMapper.Export).ToList()
        };
    }

    public Project Import(ProjectSummaryDTO foreign)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<CheckSet<ProjectSummaryDTO>> ImportChecks()
    {
        throw new NotImplementedException();
    }
}