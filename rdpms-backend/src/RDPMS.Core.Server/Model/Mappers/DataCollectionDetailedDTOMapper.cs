using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class DataCollectionDetailedDTOMapper(
    IExportMapper<MetaDataCollectionColumn, MetaDateCollectionColumnDTO> columnMapper)
    : IExportMapper<DataCollectionEntity, CollectionDetailedDTO>
{
    public CollectionDetailedDTO Export(DataCollectionEntity domain)
    {
        return new CollectionDetailedDTO
        {
            Id = domain.Id,
            Slug = domain.Slug,
            Name = domain.Name,
            Description = domain.Description,
            DataSetCount = domain.ContainedDatasets?.Count ?? 0,
            DefaultDataStoreId = domain.DefaultDataStore?.Id,
            ProjectId = domain.ParentId,
            MetaDateColumns = domain.MetaDataColumns.Select(columnMapper.Export).ToList()
        };
    }
}