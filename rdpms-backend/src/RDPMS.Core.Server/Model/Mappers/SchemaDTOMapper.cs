using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class SchemaDTOMapper
    : IExportMapper<JsonSchemaEntity, SchemaDTO>
{
    public SchemaDTO Export(JsonSchemaEntity domain)
    {
        return new SchemaDTO
        {
            Id = domain.Id,
            SchemaId = domain.SchemaId
        };
    }
}