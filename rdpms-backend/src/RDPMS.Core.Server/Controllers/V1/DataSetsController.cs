using Asp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/v{version:apiVersion}/data/datasets")]
[ApiVersion("1.0")]
public class DataSetsController(
    IDataSetService dataSetService,
    IFileService fileService,
    IS3Service s3Service,
    IStoreService storeService,
    IContentTypeService typeService,
    IDataCollectionEntityService collectionService,
    DataSetSummaryDTOMapper dataSetSummaryMapper,
    DataSetDetailedDTOMapper dataSetDetailedMapper,
    IImportMapper<DataFile, S3FileCreateRequestDTO, ContentType> s3dfCreateReqMapper,
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
    /// Add a single file to the system. Request a single S3 upload URL.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="storeId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/add/s3")]
    [ProducesResponseType<FileCreateResponseDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PostAddS3([FromRoute] Guid id, Guid? storeId,
        [FromBody] S3FileCreateRequestDTO requestDto)
    {
        if (requestDto.ContentTypeId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "ContentTypeId is required." });
        }

        if (!await dataSetService.CheckForIdAsync(id))
        {
            return BadRequest(new ErrorMessageDTO { Message = "there is no data set for the given id." });
        }

        if (!await typeService.CheckForIdAsync(requestDto.ContentTypeId.Value))
        {
            return BadRequest(new ErrorMessageDTO { Message = "there is no content type for the given id." });
        }
        var type = await typeService.GetByIdAsync(requestDto.ContentTypeId.Value);

        var dataset = await dataSetService.GetByIdAsync(id);
        if (dataset.ParentId == null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorMessageDTO { Message = "DataSet must have a parent collection." });
        }

        S3DataStore? store;
        if (storeId == null)
        {
            var collection = await collectionService.GetByIdAsync(dataset.ParentId!.Value);
            if (collection.DefaultDataStore is S3DataStore s3Store) store = s3Store;
            else return BadRequest(new ErrorMessageDTO
            {
                Message = "Collection must have a default S3 data store, or you must provide a storeId."
            });
        }
        else
        {
            var requestedStore = await storeService.GetByIdAsync(storeId.Value);
            if (requestedStore is not S3DataStore s3Store)
            {
                return BadRequest(new ErrorMessageDTO
                {
                    Message = "StoreId must refer to an S3 data store."
                });
            }
            store = s3Store;
        }

        // var url = s3Service.RequestPresignedUploadUrlAsync(store, file.Name);
        
        var requestedFile = s3dfCreateReqMapper.Import(requestDto, type);
        var reference = requestedFile.Locations.Single() as S3FileStorageReference ??
                        throw new InvalidOperationException();
        var response = await storeService.RequestS3FileUploadAsync(
            requestedFile, reference, id, store.Id);
        await fileService.AddAsync(requestedFile);
        var target = FileCreateResponseDTOMapper.ToDTO(response);
        
        return Ok(target);
    }

    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="dataSetId"></param>
    // /// <param name="requestDto"></param>
    // /// <returns></returns>
    // [HttpPost("{dataSetId:guid}/add/static")]
    // [ProducesResponseType(StatusCodes.Status204NoContent)]
    // [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    // [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status500InternalServerError)]
    // public async Task<ActionResult> PostAddStatic(Guid dataSetId, [FromBody] FileCreateRequestDTO requestDto)
    // {
    //
    // }
    //
    // /// <summary>
    // /// 
    // /// </summary>
    // /// <param name="dataSetId"></param>
    // /// <returns></returns>
    // [HttpPost("{dataSetId:guid}/finalize")]
    // public async Task<ActionResult> PostFinalize(Guid dataSetId)
    // {
    //     
    // }

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