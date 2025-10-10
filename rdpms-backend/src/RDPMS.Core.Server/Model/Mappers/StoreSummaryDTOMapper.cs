using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class StoreSummaryDTOMapper
    : IExportMapper<DataStore, DataStoreSummaryDTO>
{
    public DataStoreSummaryDTO Export(DataStore domain)
    {
        return new DataStoreSummaryDTO
        {
            Id = domain.Id ,
            Slug = domain.Slug,
            Name = domain.Name,
            StorageType = domain.StorageType.ToString().ToLower(),
            PropertiesJson = domain.GetPublicInfoContentJson()
        };
    }

    public IEnumerable<CheckSet<DataStoreSummaryDTO>> ImportChecks() => [];
}