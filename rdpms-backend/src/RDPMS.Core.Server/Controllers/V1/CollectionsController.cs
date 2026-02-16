using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/v{version:apiVersion}/data/collections")]
[ApiVersion("1.0")]
public class CollectionsController(
    IDataCollectionEntityService dataCollectionEntityService,
    IStoreService storeService,
    IProjectService projectService,
    DataCollectionSummaryDTOMapper cMapper,
    IExportMapper<DataCollectionEntity, CollectionDetailedDTO> cDetailedMapper,
    ILogger<CollectionsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all collections.
    /// </summary>
    /// <param name="projectId">Used to filter for collections with this parent project</param>
    /// <param name="projectSlug">Used to filter for collections with this parent project</param>
    /// <param name="slug">Used to filter for collections with this slug</param>   
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CollectionSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CollectionSummaryDTO>>> Get(
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? projectSlug = null,
        [FromQuery] string? slug = null)
    {
        var list = await dataCollectionEntityService.GetAllAsync();
        var query = list.AsQueryable();
        if (projectId != null)
        {
            query = query.Where(c => c.ParentId == projectId);
        }
        if (projectSlug != null)
        {
            var project = (await projectService.GetAllAsync()).SingleOrDefault(p => p.Slug == projectSlug);

            if (project == null) return Ok(Array.Empty<CollectionSummaryDTO>());
            
            query = query.Where(c => c.ParentId == project.Id);
        }
        if (slug != null)
        {
            query = query.Where(c => c.Slug == slug);
        }
        return Ok(query.AsEnumerable().Select(cMapper.Export));
    }

    /// <summary>
    /// Get a single collection by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CollectionDetailedDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionDetailedDTO>> GetById([FromRoute] Guid id)
    {
        try
        {
            var item = await dataCollectionEntityService.GetByIdAsync(id);
            return Ok(cDetailedMapper.Export(item));
        }
        catch (InvalidOperationException e)
        {
            logger.LogDebug("Failed to retrieve datasets for collection {CollectionId}, {EMessage}", id, e);
            return NotFound($"Collection {id} was not found.");
        }
    }
    
    
    /// <summary>
    /// Add a single item to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] CollectionSummaryDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "Id is not allowed to be set." });
        }

        if (dto.DefaultDataStoreId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "Default data store id may not be null." });
        };

        var exists = await storeService.CheckForIdAsync(dto.DefaultDataStoreId.Value);
        if (!exists)
        {
            return NotFound(
                new ErrorMessageDTO { Message = "Could not look up id for default data store. Id does not exist." });
        }

        var store = await storeService.GetByIdAsync(dto.DefaultDataStoreId.Value);
        var project = await projectService.GetGlobalProjectAsync();
        await dataCollectionEntityService.AddAsync(cMapper.Import(dto, store, project));
        return Ok();
    }

    [HttpPut("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpsertMetadataColumn([FromRoute] Guid id, [FromRoute] string key,
        [FromQuery] Guid schemaId,
        [FromQuery] Guid? defaultMetadataId = null,
        [FromQuery] MetadataColumnTargetDTO target = MetadataColumnTargetDTO.Dataset)
    {
        var collectionExists = await dataCollectionEntityService.CheckForIdAsync(id);
        if (!collectionExists)
        {
            return NotFound(new ErrorMessageDTO { Message = "No collection with that id." });
        }

        var targetDto = (MetadataColumnTarget)(int)target;
        var created = await dataCollectionEntityService.UpsertMetaDataColumnAsync(
            id, key, schemaId, defaultMetadataId, targetDto);

        return created
            ? CreatedAtAction(nameof(GetById), new { id }, id)
            : Ok();
    }

    // TODO: Docstring, DELETE...
    [HttpPost("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RenameMetadataColumn([FromRoute] Guid id, [FromRoute] string key,
        [FromQuery] string newKey,
        [FromQuery] MetadataColumnTargetDTO target = MetadataColumnTargetDTO.Dataset)
    {
        try
        {
            var targetDto = (MetadataColumnTarget)(int)target;
            await dataCollectionEntityService.RenameColumnAsync(id, key, newKey, targetDto);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorMessageDTO { Message = "Tuple of id and key not found." });
        }
    }

    [HttpDelete("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteMetadataColumn(
        [FromRoute] Guid id,
        [FromRoute] string key,
        [FromQuery] MetadataColumnTargetDTO target = MetadataColumnTargetDTO.Dataset)
    {
        try
        {
            var targetDto = (MetadataColumnTarget)(int)target;
            await dataCollectionEntityService.DeleteColumnAsync(id, key, targetDto);
            return Ok();
        }
        catch (InvalidOperationException e)
        {
            return NotFound(new ErrorMessageDTO { Message = "Tuple of id and key not found." });
        }
    }
}
