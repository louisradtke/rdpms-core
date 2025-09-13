using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class DataSetsController(
    IDataSetService dataSetService,
    DataSetSummaryDTOMapper dataSetMapper,
    ILogger<DataSetsController> logger)
    : ControllerBase
{
    [HttpGet("datasets")]
    [ProducesResponseType<IEnumerable<DataSetSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataSetSummaryDTO>>> Get()
    {
        var list = await dataSetService.GetAllAsync();
        return Ok(list.Select(dataSetMapper.Export));
    }

    [HttpGet("datasets/{id:guid}")]
    [ProducesResponseType<DataSetSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataSetSummaryDTO>> GetById([FromRoute] Guid id)
    {
        var item = await dataSetService.GetByIdAsync(id);
        return Ok(dataSetMapper.Export(item));
    }

    /// <summary>
    /// Add a single item to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("datasets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Post([FromBody] DataSetSummaryDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await dataSetService.AddAsync(dataSetMapper.Import(dto));
        return Ok();
    }

    /// <summary>
    /// Add a batch of item to the system.
    /// </summary>
    /// <param name="dtos"></param>
    /// <returns></returns>
    [HttpPost("datasets/batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult> PostBatch([FromBody] IEnumerable<DataSetSummaryDTO> dtos)
    {
        var dtosList = dtos.ToList();
        if (dtosList.Any(d => d.Id != null))
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await dataSetService.AddRangeAsync(dtosList.Select(dataSetMapper.Import));
        return Ok();
    }

    /// <summary>
    /// Get data sets in a collection
    /// </summary>
    /// <param name="collectionId"></param>
    /// <returns></returns>
    [HttpGet("collections/{collectionId:guid}/datasets")]
    [ProducesResponseType<IEnumerable<DataSetSummaryDTO>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> GetByCollectionId(Guid collectionId)
    {
        try
        {
            var datasets = await dataSetService.GetByCollectionAsync(collectionId);
            return Ok(datasets.Select(dataSetMapper.Export));
        }
        catch (InvalidOperationException e)
        {
            logger.LogDebug("Failed to retrieve datasets for collection {CollectionId}, {EMessage}", collectionId, e);
            return NotFound($"Collection {collectionId} was not found.");
        }
    }

}