using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class DataCollectionSummaryDTOMapper
    : IExportMapper<DataCollectionEntity, CollectionSummaryDTO>,
        IImportMapper<DataCollectionEntity, CollectionSummaryDTO, DataStore, Project>
{
    private readonly List<CheckSet<CollectionSummaryDTO>> _importChecks;

    public DataCollectionSummaryDTOMapper()
    {
        _importChecks = new List<CheckSet<CollectionSummaryDTO>>();

        // Adding required checks for import operation
        _importChecks.Add(CheckSet<CollectionSummaryDTO>.CreateErr(
            dto => dto.Id != null, _ => "Id may not be set. It will be set by the server."));
        _importChecks.Add(CheckSet<CollectionSummaryDTO>.CreateErr(
            dto => !string.IsNullOrWhiteSpace(dto.Name), _ => "Name is required."));
        _importChecks.Add(CheckSet<CollectionSummaryDTO>.CreateErr(
            dto => dto.DefaultDataStoreId != null, 
            _ => "DefaultDataStoreId is required for this collection."));
        _importChecks.Add(CheckSet<CollectionSummaryDTO>.CreateWarn(
            dto => dto.DataSetCount is null or >= 0,
            _ => "DataFilesCount should be a valid number or will be ignored by the server."));
        _importChecks.Add(CheckSet<CollectionSummaryDTO>.CreateErr(
            dto => dto.Slug is null || SlugUtil.IsValidSlug(dto.Slug),
            _ => "Slug is not valid."));
    }

    public DataCollectionEntity Import(CollectionSummaryDTO dto, DataStore defaultStore, Project parentProject)
    {
        foreach (var checkSet in _importChecks)
        {
            if (checkSet.Severity <= ErrorSeverity.Error || !checkSet.CheckFunc(dto))
                continue;

            throw new IllegalArgumentException(checkSet.MessageFunc(dto));
        }

        var collection = new DataCollectionEntity(
            dto.Name ?? throw new IllegalStateException("Name is required."))
        {
            Id = Guid.NewGuid(),
            Slug = dto.Slug,
            Name = dto.Name,
            Description = dto.Description,
            ContainedDatasets = new List<DataSet>(),
            DefaultDataStore = defaultStore,
            ParentId = parentProject.Id
        };

        return collection;
    }

    public CollectionSummaryDTO Export(DataCollectionEntity domain)
    {
        return new CollectionSummaryDTO
        {
            Id = domain.Id,
            Slug = domain.Slug,
            Name = domain.Name,
            Description = domain.Description,
            DataSetCount = domain.ContainedDatasets?.Count ?? 0,
            DefaultDataStoreId = domain.DefaultDataStore?.Id,
            ProjectId = domain.ParentId
        };
    }

    public IEnumerable<CheckSet<CollectionSummaryDTO>> ImportChecks() => _importChecks;
}