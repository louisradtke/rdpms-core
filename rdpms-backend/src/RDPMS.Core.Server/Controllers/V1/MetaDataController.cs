using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/data")]
[ApiVersion("1.0")]
public class MetaDataController(
    IMetadataService metadataService,
    ISchemaService schemaService,
    IExportMapper<MetadataJsonField, MetaDateDTO> metadataMapper,
    LinkGenerator linkGenerator,
    ILogger<DataSetsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Get all metadata. Contents can be retrieved via Files API.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Get a single meta date by id. Content can be retrieved via Files API.
    /// </summary>
    /// <param name="id">Id of the meta date</param>
    /// <returns></returns>
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

    /// <summary>
    /// Validate meta date against a schema. On success, the meta date gets updated and true gets returned.
    /// If meta date cannot be validated, false gets returned.
    /// </summary>
    /// <param name="id">ID </param>
    /// <param name="schemaId"></param>
    /// <returns></returns>
    [HttpPut("metadata/{id:guid}/validate/{schemaId:guid}")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ValidateMetadata([FromRoute] Guid id, [FromRoute] Guid schemaId)
    {
        var result = await metadataService.VerifySchema(id, schemaId);
        return Ok(result);
    }

    /// <summary>
    /// Get all schemas registered in the system.
    /// </summary>
    /// <returns>List of all schemas.</returns>
    [HttpGet("schemas")]
    [ProducesResponseType<IDictionary<Guid, string>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSchemas()
    {
        return Ok(
            (await schemaService.GetAllAsync())
            .ToDictionary(s => s.Id, s => s.SchemaId)
        );
    }

    /// <summary>
    /// Add a new schema to the system.
    /// </summary>
    /// <param name="schemaId">(optional) Schema URL, if existing. Keep empty otherwise.</param>
    /// <param name="value">The JSON document</param>
    /// <returns>HTTP 201, if schema was successfully created.
    /// HTTP 400, if URL is invalid or schema with ID (URL) already exists.</returns>
    /// <exception cref="InvalidOperationException">On internal error.</exception>
    [HttpPost("schemas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddSchema([FromQuery] string? schemaId, [FromBody] byte[] value)
    {
        if (schemaId != null)
        {
            if (!Uri.IsWellFormedUriString(schemaId, UriKind.Absolute))
            {
                return BadRequest(new ErrorMessageDTO { Message = "Invalid schemaId: must be a valid URL." });
            }

            var exists = await schemaService.Query()
                .AnyAsync(s => s.SchemaId == schemaId);
            if (exists)
            {
                return BadRequest(new ErrorMessageDTO { Message = "Schema with that id already exists." });
            }
        }

        var schemaGuid = Guid.NewGuid();
        if (schemaId == null)
        {
            var version = HttpContext.Request.RouteValues["version"]; // hack required for ominous reasons
            schemaId = linkGenerator.GetUriByAction(
                HttpContext, nameof(GetRawSchema), "MetaData",
                new { id = schemaGuid, version });
        }

        if (schemaId == null)
        {
            logger.LogError("Failed to generate schemaId link.");
            throw new InvalidOperationException("Failed to generate schemaId link.");
        }

        var schema = new JsonSchemaEntity()
        {
            Id = schemaGuid,
            SchemaId = schemaId,
            SchemaString = Encoding.UTF8.GetString(value)
        };
        await schemaService.AddAsync(schema);
        return CreatedAtAction(nameof(GetRawSchema), new { id = schema.Id }, schema.Id);
    }

    /// <summary>
    /// Get the raw value of a schema.
    /// </summary>
    /// <param name="id">Guid of the schema.</param>
    /// <returns>
    /// HTTP 200, if schema was found. HTTP 404 otherwise.</returns>
    [HttpGet("schemas/{id:guid}/blob")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/json")]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetRawSchema([FromRoute] Guid id)
    {
        try
        {
            var item = await schemaService.GetByIdAsync(id);
            return File(Encoding.UTF8.GetBytes(item.SchemaString), "application/json",
                $"{item.Id}.json");
        }
        catch (InvalidOperationException)
        {
            return NotFound($"Metadata with id {id} was not found.");
        }
    }
}