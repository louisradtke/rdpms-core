using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class ProjectSummaryDTOMapper(
    IExportMapper<DataStore, DataStoreSummaryDTO> dsExportMapper,
    IExportMapper<DataCollectionEntity, CollectionSummaryDTO> dcExportMapper)
    : IExportMapper<Project, ProjectSummaryDTO>
{
    public ProjectSummaryDTO Export(Project domain)
    {
        return new ProjectSummaryDTO()
        {
            Id = domain.Id,
            Name = domain.Name,
            DataStores = domain.DataStores.Select(dsExportMapper.Export).ToList(),
            Collections = domain.DataCollections.Select(dcExportMapper.Export).ToList()
        };
    }
}