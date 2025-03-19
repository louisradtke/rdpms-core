using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Repositories;

namespace RDPMS.Core.Server.Model.Mappers;

public class ContainerSummaryDTOMapper
    : IExportMapper<DataContainer, ContainerSummaryDTO>,
        IImportMapper<DataContainer, ContainerSummaryDTO, DataStore>
{
    private readonly List<CheckSet<ContainerSummaryDTO>> _importChecks;

    public ContainerSummaryDTOMapper()
    {
        _importChecks = new List<CheckSet<ContainerSummaryDTO>>();

        // Adding required checks for import operation
        _importChecks.Add(CheckSet<ContainerSummaryDTO>.CreateErr(
            dto => dto.Id != null, _ => "Id may not be set. It will be set by the server."));
        _importChecks.Add(CheckSet<ContainerSummaryDTO>.CreateErr(
            dto => !string.IsNullOrWhiteSpace(dto.Name), _ => "Name is required."));
        _importChecks.Add(CheckSet<ContainerSummaryDTO>.CreateErr(
            dto => dto.DefaultDataStoreId != null, 
            _ => "DefaultDataStoreId is required for this container."));
        _importChecks.Add(CheckSet<ContainerSummaryDTO>.CreateWarn(
            dto => dto.DataFilesCount == null || dto.DataFilesCount >= 0,
            _ => "DataFilesCount should be a valid number or will be ignored by the server."));
    }

    public DataContainer Import(ContainerSummaryDTO foreign, DataStore defaultStore)
    {
        foreach (var checkSet in _importChecks)
        {
            if (checkSet.Severity >= ErrorSeverity.Error && checkSet.CheckFunc(foreign))
                continue;

            throw new ArgumentException(checkSet.MessageFunc(foreign));
        }

        var container = new DataContainer(foreign.Name ?? throw new IllegalStateException("Name is required."))
        {
            Id = Guid.NewGuid(),
            AssociatedDataSets = new List<DataSet>(),
            DefaultDataStore = defaultStore
        };

        return container;
    }

    public ContainerSummaryDTO Export(DataContainer domain)
    {
        return new ContainerSummaryDTO
        {
            Id = domain.Id,
            Name = domain.Name,
            DataFilesCount = domain.AssociatedDataSets?.Count ?? 0,
            DefaultDataStoreId = domain.DefaultDataStore.Id
        };
    }

    public IEnumerable<CheckSet<ContainerSummaryDTO>> ImportChecks() => _importChecks;
}