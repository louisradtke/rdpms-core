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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] CollectionSummaryDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest("Id is not allowed to be set.");
        }

        if (dto.DefaultDataStoreId == null)
            return BadRequest("Default data store id may not be null.");;

        var exists = await storeService.CheckForIdAsync(dto.DefaultDataStoreId.Value);
        if (!exists)
            return BadRequest("Could not look up id for default data store. Id does not exist.");

        var store = await storeService.GetByIdAsync(dto.DefaultDataStoreId.Value);
        var project = await projectService.GetGlobalProjectAsync();
        await dataCollectionEntityService.AddAsync(cMapper.Import(dto, store, project));
        return Ok();
    }

    
    [HttpPut("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AddMetaDataColumn([FromRoute] Guid id, [FromRoute] string key,
        [FromBody] Guid schemaId, [FromQuery] Guid? defaultMetadataId = null)
    {
        var collectionExists = await dataCollectionEntityService.CheckForIdAsync(id);
        if (!collectionExists)
        {
            return NotFound();
        }

        var created = await dataCollectionEntityService.UpsertMetaDataColumnAsync(
            id, key, schemaId, defaultMetadataId);

        return created
            ? CreatedAtAction(nameof(GetById), new { id }, id)
            : Ok();
    }
}
