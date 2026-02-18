using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class SchemaValidationResultDTOMapper : IExportMapper<ValidationResult, SchemaValidationResultDTO>
{
    public SchemaValidationResultDTO Export(ValidationResult domain)
    {
        return new SchemaValidationResultDTO
        {
            Succesful = domain.Succesful,
            Reasons = domain.Reasons,
            Traces = domain.Traces
        };
    }
}
