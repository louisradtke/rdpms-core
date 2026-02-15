using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class MetaDateCollectionColumnDTOMapper(
    IExportMapper<JsonSchemaEntity, SchemaDTO> schemaMapper)
    :IExportMapper<MetaDataCollectionColumn, MetaDateCollectionColumnDTO>
{
    public MetaDateCollectionColumnDTO Export(MetaDataCollectionColumn domain)
    {
        return new MetaDateCollectionColumnDTO
        {
            DefaultFieldId = domain.DefaultFieldId,
            MetadataKey = domain.MetadataKey,
            Target = domain.Target,
            Schema = domain.Schema != null ? schemaMapper.Export(domain.Schema) : null
        };
    }
}
