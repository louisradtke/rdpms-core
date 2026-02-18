using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;
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
    IExportMapper<JsonSchemaEntity, SchemaDTO> schemaMapper,
    IExportMapper<ValidationResult, SchemaValidationResultDTO> schemaValidationResultMapper)
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
    /// Validate meta date against a schema and return a detailed result.
    /// If verbose mode is enabled, validator traces are included.
    /// </summary>
    /// <param name="id">ID </param>
    /// <param name="schemaId"></param>
    /// <param name="verbose"></param>
    /// <returns></returns>
    [HttpPut("metadata/{id:guid}/validate/{schemaId:guid}")]
    [ProducesResponseType<SchemaValidationResultDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SchemaValidationResultDTO>> ValidateMetadata([FromRoute] Guid id, [FromRoute] Guid schemaId, [FromQuery] bool? verbose = false)
    {
        var result = await metadataService.VerifySchema(id, schemaId, verbose ?? false);
        return Ok(schemaValidationResultMapper.Export(result));
    }

    /// <summary>
    /// Get all schemas registered in the system.
    /// </summary>
    /// <returns>List of all schemas.</returns>
    [HttpGet("schemas")]
    [ProducesResponseType<IEnumerable<SchemaDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetSchemas()
    {
        var schemas = await schemaService.GetAllAsync();
        var dtos = schemas.Select(schemaMapper.Export).AsEnumerable();
        return Ok(dtos);
    }

    /// <summary>
    /// Add a new schema to the system.
    /// </summary>
    /// <param name="value">The JSON document</param>
    /// <returns>HTTP 201, if schema was successfully created.
    /// HTTP 400, if URL is invalid or schema with ID (URL) already exists.</returns>
    /// <exception cref="InvalidOperationException">On internal error.</exception>
    [HttpPost("schemas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddSchema([FromBody] JsonElement value)
    {
        if (!TryValidateSchemaPayload(value, out var schemaString))
        {
            return BadRequest(new ErrorMessageDTO
            {
                Message = "Schema payload must be a valid JSON object."
            });
        }

        if (!TryReadSchemaId(value, out var schemaIdFromDocument, out var schemaIdReadError))
        {
            return BadRequest(new ErrorMessageDTO { Message = schemaIdReadError });
        }

        var schemaGuid = Guid.NewGuid();
        var effectiveSchemaId = schemaIdFromDocument;
        if (effectiveSchemaId != null && !Uri.IsWellFormedUriString(effectiveSchemaId, UriKind.Absolute))
        {
            return BadRequest(new ErrorMessageDTO { Message = "Invalid schemaId: must be a valid URL." });
        }

        if (effectiveSchemaId == null)
        {
            effectiveSchemaId = $"urn:rdpms:3rdparty:schema:{schemaGuid.ToString().ToLowerInvariant()}";
            if (!TryInjectSchemaId(value, effectiveSchemaId, out var schemaWithId))
            {
                return BadRequest(new ErrorMessageDTO { Message = "Schema payload must be a valid JSON object." });
            }

            schemaString = schemaWithId;
        }

        var exists = await schemaService.Query()
            .AnyAsync(s => s.SchemaId == effectiveSchemaId);
        if (exists)
        {
            return BadRequest(new ErrorMessageDTO { Message = "Schema with that id already exists." });
        }

        var schema = new JsonSchemaEntity()
        {
            Id = schemaGuid,
            SchemaId = effectiveSchemaId,
            SchemaString = schemaString
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

    private static bool TryValidateSchemaPayload(JsonElement payload, out string schemaString)
    {
        schemaString = string.Empty;
        if (payload.ValueKind is not JsonValueKind.Object)
        {
            return false;
        }

        schemaString = payload.GetRawText();
        return true;
    }

    private static bool TryReadSchemaId(JsonElement payload, out string? schemaId, out string? error)
    {
        schemaId = null;
        error = null;

        if (payload.ValueKind != JsonValueKind.Object)
        {
            error = "Schema payload must be a valid JSON object.";
            return false;
        }

        if (!payload.TryGetProperty("$id", out var idElement))
        {
            return true;
        }

        if (idElement.ValueKind != JsonValueKind.String)
        {
            error = "Schema `$id` must be a string.";
            return false;
        }

        var id = idElement.GetString();
        if (string.IsNullOrWhiteSpace(id))
        {
            error = "Schema `$id` must not be empty.";
            return false;
        }

        schemaId = id;
        return true;
    }

    private static bool TryInjectSchemaId(JsonElement payload, string schemaId, out string schemaWithId)
    {
        schemaWithId = string.Empty;
        var node = JsonNode.Parse(payload.GetRawText());
        if (node is not JsonObject obj) return false;

        obj["$id"] = schemaId;
        schemaWithId = obj.ToJsonString();
        return true;
    }
}
