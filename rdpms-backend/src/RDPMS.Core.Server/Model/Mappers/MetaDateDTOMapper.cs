using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister(registerFlags: RegisterFlags.ShallowInterfaces | RegisterFlags.Self)]
public class MetaDateDTOMapper(IExportMapper<ContentType, ContentTypeDTO> contentTypeMapper)
    : IExportMapper<MetadataJsonField, MetaDateDTO>
{
    public MetaDateDTO Export(MetadataJsonField domain)
    {
        if (domain.Value == null) throw new ArgumentNullException(nameof(domain.Value));
        return new MetaDateDTO
        {
            Id = domain.Id,
            FileContentType = contentTypeMapper.Export(domain.Value.FileType),
            FileId = domain.ValueId,
            ValidatedSchemaIds = domain.ValidatedSchemas.Select(s => s.SchemaId).ToList()
        };
    }
}