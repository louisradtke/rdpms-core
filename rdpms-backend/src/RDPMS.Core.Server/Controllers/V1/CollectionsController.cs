using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data")]
[ApiVersion("1.0")]
public class CollectionsController(
    IDataCollectionEntityService dataCollectionEntityService,
    IStoreService storeService,
    IProjectService projectService,
    DataCollectionSummaryDTOMapper cMapper,
    ILogger<CollectionsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all colletions.
    /// </summary>
    /// <returns></returns>
    [HttpGet("collections")]
    [ProducesResponseType<IEnumerable<CollectionSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CollectionSummaryDTO>>> Get()
    {
        var list = await dataCollectionEntityService.GetAllAsync();
        return Ok(list.Select(cMapper.Export));
    }

    /// <summary>
    /// Get a single collection by id.
    /// </summary>
    [HttpGet("collections/{id:guid}")]
    [ProducesResponseType<CollectionSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CollectionSummaryDTO>> GetById([FromRoute] Guid id)
    {
        try
        {
            var item = await dataCollectionEntityService.GetByIdAsync(id);
            return Ok(cMapper.Export(item));
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
    [HttpPost("collections")]
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
}