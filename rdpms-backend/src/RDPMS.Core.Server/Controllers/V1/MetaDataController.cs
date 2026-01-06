using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/data")]
[ApiVersion("1.0")]
public class MetaDataController(
    IMetadataService metadataService,
    ISchemaService schemaService,
    IExportMapper<MetadataJsonField, MetaDateDTO> metadataMapper,
    ILogger<DataSetsController> logger)
    : ControllerBase
{
    [HttpGet("metadata")]
    [ProducesResponseType<IEnumerable<MetaDateDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetMetadates()
    {
        return Ok(
            (await metadataService.GetAllAsync())
            .Select(metadataMapper.Export)
            .AsEnumerable()
        );
    }

    [HttpGet("metadata/{id:guid}")]
    [ProducesResponseType<MetaDateDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetMetadate([FromRoute] Guid id)
    {
        try
        {
            var item = await metadataService.GetByIdAsync(id);
            return Ok(metadataMapper.Export(item));
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Metadata with id {id} was not found.");
        }
    }
    
    [HttpGet("meta-schema")]
    [ProducesResponseType<IDictionary<Guid, string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSchemata()
    {
        return Ok(
            (await schemaService.GetAllAsync())
            .ToDictionary(s => s.Id, s => s.SchemaId)
        );
    }

    [HttpGet("meta-schema/{id:guid}/blob")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/json")]
    public async Task<ActionResult> GetRawSchema([FromRoute] Guid id)
    {
        try
        {
            var item = await schemaService.GetByIdAsync(id);
            return Ok(item.SchemaString);
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Metadata with id {id} was not found.");
        }
    }
}