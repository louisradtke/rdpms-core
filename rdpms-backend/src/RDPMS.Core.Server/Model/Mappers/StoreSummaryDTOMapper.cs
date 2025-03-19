using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

public class StoreSummaryDTOMapper
    : IExportMapper<DataStore, DataStoreSummaryDTO>,
        IImportMapper<DataStore, DataStoreSummaryDTO>
{
    public DataStoreSummaryDTO Export(DataStore domain)
    {
        return new DataStoreSummaryDTO
        {
            Id = domain.Id ,
            Name = domain.Name,
            DataFilesCount = domain.DataFiles?.Count
        };
    }

    public DataStore Import(DataStoreSummaryDTO foreign)
    {
        if (foreign == null)
        {
            throw new ArgumentNullException(nameof(foreign));
        }

        return new DataStore(foreign.Name ?? string.Empty)
        {
            Id = foreign.Id ?? Guid.NewGuid(),
            DataFiles = []
        };
    }

    public IEnumerable<CheckSet<DataStoreSummaryDTO>> ImportChecks() => [];
}