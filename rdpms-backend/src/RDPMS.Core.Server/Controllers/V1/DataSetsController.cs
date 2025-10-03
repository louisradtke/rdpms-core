using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data/datasets")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class DataSetsController(
    IDataSetService dataSetService,
    IFileService fileService,
    DataSetSummaryDTOMapper dataSetSummaryMapper,
    DataSetDetailedDTOMapper dataSetDetailedMapper,
    LinkGenerator linkGenerator,
    ILogger<DataSetsController> logger)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<DataSetSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataSetSummaryDTO>>> Get([FromQuery] Guid? collectionId = null)
    {
        IEnumerable<DataSet> datasets;
        try
        {
            if (collectionId != null) datasets = await dataSetService.GetByCollectionAsync(collectionId.Value);
            else datasets = await dataSetService.GetAllAsync();
        }
        catch (InvalidOperationException e)
        {
            logger.LogDebug("Failed to retrieve datasets for collection {CollectionId}, {EMessage}", collectionId, e);
            return NotFound($"Collection {collectionId} was not found.");
        }

        var dtos = datasets.Select(dataSetSummaryMapper.Export);
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<DataSetDetailedDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataSetDetailedDTO>> GetById([FromRoute] Guid id)
    {
        var domainItem = await dataSetService.GetByIdAsync(id);
        var dto = dataSetDetailedMapper.Export(domainItem);
        foreach (var file in dto.Files)
        {
            file.DownloadURI = fileService.GetContentApiUri(file.Id!.Value, HttpContext);
        }
        return Ok(dto);
    }

    /// <summary>
    /// Add a single item to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Post([FromBody] DataSetSummaryDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await dataSetService.AddAsync(dataSetSummaryMapper.Import(dto));
        return Ok();
    }

    /// <summary>
    /// Add a batch of item to the system.
    /// </summary>
    /// <param name="dtos"></param>
    /// <returns></returns>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult> PostBatch([FromBody] IEnumerable<DataSetSummaryDTO> dtos)
    {
        var dtosList = dtos.ToList();
        if (dtosList.Any(d => d.Id != null))
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await dataSetService.AddRangeAsync(dtosList.Select(dataSetSummaryMapper.Import));
        return Ok();
    }
}