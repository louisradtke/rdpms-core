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
            Name = domain.Name,
            DataFilesCount = domain.DataFiles?.Count,
            StorageType = domain.StorageType.ToString().ToLower()
        };
    }

    public IEnumerable<CheckSet<DataStoreSummaryDTO>> ImportChecks() => [];
}