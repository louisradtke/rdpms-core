using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data/files")]
[ApiVersion("1.0")]
public class FilesController(
    IFileService fileService,
    IContentTypeService typeService,
    FileSummaryDTOMapper fileMapper,
    FileCreateRequestDTOMapper dfCreateReqMapper,
    LinkGenerator linkGenerator,
    ILogger<FilesController> logger) : ControllerBase
{
    /// <summary>
    /// Get summaries of all files.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<FileSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> Get()
    {
        var list = (await fileService.GetAllAsync()).Select(fileMapper.Export).ToList();
        foreach (var file in list)
        {
            file.DownloadURI = linkGenerator.GetUriByAction(
                HttpContext, nameof(GetContent), null,
                new {id = file.Id});
        }
        return Ok(list);
    }

    /// <summary>
    /// Request an upload for a new file.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<FileCreateResponseDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PostNewFile([FromBody] FileCreateRequestDTO requestDto)
    {
        if (requestDto.ContentTypeId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "ContentTypeId is required." });
        }
        
        if (requestDto.AssociatedDataSetId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "AssociatedDataSetId is required." });
        }

        if (!await typeService.CheckForIdAsync(requestDto.ContentTypeId.Value))
        {
            return BadRequest(new ErrorMessageDTO { Message = "there is no content type for the given id." });
        }

        var type = await typeService.GetByIdAsync(requestDto.ContentTypeId.Value);
        var request = dfCreateReqMapper.Import(requestDto, type);
        var response = await fileService.RequestFileUploadAsync(request);
        var target = FileCreateResponseDTOMapper.ToDTO(response);
        
        var resourceUri = 
            Url.Action(nameof(Get), null, new {id = target.FileId}, Request.Scheme);

        return Created(resourceUri, target);
    }

    /// <summary>
    /// Get summary of a single file.
    /// </summary>
    /// <param name="id">Id of the file.</param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FileSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileSummaryDTO>> Get(Guid id)
    {
        try
        {
            var file = await fileService.GetByIdAsync(id);
            return Ok(fileMapper.Export(file));
        }
        catch (InvalidOperationException e)
        {
            logger.LogDebug("Failed to retrieve datasets for collection {CollectionId}, {EMessage}", id, e);
            return NotFound(new ErrorMessageDTO { Message = $"File {id} was not found."});
        }
    }

    /// <summary>
    /// Request redirect to a file.
    /// On success, a redirect will be returned.
    /// </summary>
    /// <param name="id">Id of the file to download.</param>
    /// <returns>On success, a redirect will be returned.</returns>
    [HttpGet("{id:guid}/content")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [EnableCors("ExternalCorsPolicy")] // needed for redirect
    public async Task<ActionResult> GetContent(Guid id)
    {
        try
        {
            var uri = await fileService.GetFileDownloadUriAsync(id, HttpContext);

            return Redirect(uri.ToString());

            // // Otherwise return the file content
            // var fileStream = await fileService.GetFileContentAsync(id);
            // return File(fileStream, uri.ContentType?.MimeType ?? "application/octet-stream", uri.Name);
        }
        catch (InvalidOperationException e)
        {
            logger.LogDebug("Failed to retrieve file {FileId}, {EMessage}", id, e);
            return NotFound(new ErrorMessageDTO { Message = $"File {id} was not found."});
        }
    }
    
    /// <summary>
    /// Request a download of a file.
    /// On success, either a redirect or a file download (the raw bytes) will be returned.
    /// </summary>
    /// <param name="id">Id of the file to download.</param>
    /// <returns>On success, either a redirect or a file download (the raw bytes) will be returned.</returns>
    [HttpGet("{id:guid}/blob")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/octet-stream")]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult> GetBlob(Guid id)
    {
        return StatusCode(501, "Not Implemented");
    }

}