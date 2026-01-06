using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class SchemaService(DbContext context) : GenericCollectionService<JsonSchemaEntity>(context), ISchemaService;