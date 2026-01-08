using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister(registerFlags: RegisterFlags.ShallowInterfaces | RegisterFlags.Self)]
public class MetaDateDTOMapper(
    SchemaDTOMapper schemaMapper)
    : IExportMapper<MetadataJsonField, MetaDateDTO>
{
    public MetaDateDTO Export(MetadataJsonField domain)
    {
        if (domain.Value == null) throw new ArgumentNullException(nameof(domain.Value));
        return new MetaDateDTO
        {
            Id = domain.Id,
            FileId = domain.ValueId,
            ValidatedSchemas = domain.ValidatedSchemas
                .Select(schemaMapper.Export)
                .ToList()
        };
    }
}