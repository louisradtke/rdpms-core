using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface ISchemaService : IGenericCollectionService<JsonSchemaEntity>;