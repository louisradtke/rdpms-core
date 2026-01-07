using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Consumes("application/json")]
[Route("api/v{version:apiVersion}/data")]
[ApiVersion("1.0")]
public class FilesController(
    IFileService fileService,
    IContentTypeService typeService,
    FileSummaryDTOMapper fileMapper,
    IExportMapper<FileStorageReference, FileStorageReferenceSummaryDTO> referenceMapper,
    FileCreateRequestDTOMapper dfCreateReqMapper,
    LinkGenerator linkGenerator,
    ILogger<FilesController> logger) : ControllerBase
{
    /// <summary>
    /// Get summaries of all files.
    /// </summary>
    /// <returns></returns>
    [HttpGet("files")]
    [ProducesResponseType<IEnumerable<FileSummaryDTO>>(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> Get()
    {
        var list = (await fileService.GetAllAsync()).Select(fileMapper.Export).ToList();
        foreach (var file in list)
        {
            if (file.Id == null) throw new IllegalStateException("File has null id");

            file.DownloadURI = fileService.GetContentApiUri(file.Id.Value, HttpContext);
        }
        return Ok(list);
    }

    // /// <summary>
    // /// Request an upload for a new file.
    // /// </summary>
    // /// <param name="requestDto"></param>
    // /// <returns></returns>
    // [HttpPost("files")]
    // [ProducesResponseType<FileCreateResponseDTO>(StatusCodes.Status201Created)]
    // [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    // public async Task<ActionResult> PostNewFile([FromBody] FileCreateRequestDTO requestDto)
    // {
    //     if (requestDto.ContentTypeId == null)
    //     {
    //         return BadRequest(new ErrorMessageDTO { Message = "ContentTypeId is required." });
    //     }
    //     
    //     if (requestDto.AssociatedDataSetId == null)
    //     {
    //         return BadRequest(new ErrorMessageDTO { Message = "AssociatedDataSetId is required." });
    //     }
    //
    //     if (!await typeService.CheckForIdAsync(requestDto.ContentTypeId.Value))
    //     {
    //         return BadRequest(new ErrorMessageDTO { Message = "there is no content type for the given id." });
    //     }
    //
    //     var type = await typeService.GetByIdAsync(requestDto.ContentTypeId.Value);
    //     var request = dfCreateReqMapper.Import(requestDto, type);
    //     var response = await fileService.RequestFileUploadAsync(request);
    //     var target = FileCreateResponseDTOMapper.ToDTO(response);
    //     
    //     var resourceUri = 
    //         Url.Action(nameof(Get), null, new {id = target.FileId}, Request.Scheme);
    //
    //     return Created(resourceUri, target);
    // }

    /// <summary>
    /// Get summary of a single file.
    /// </summary>
    /// <param name="id">Id of the file.</param>
    /// <returns></returns>
    [HttpGet("files/{id:guid}")]
    [ProducesResponseType<FileSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
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
    /// On success, a 302 referring to the final download URL will be returned.
    /// </summary>
    /// <param name="id">Id of the file to download.</param>
    /// <returns>On success, a redirect will be returned.</returns>
    [HttpGet("files/{id:guid}/content")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [EnableCors("ExternalCorsPolicy")] // needed for redirect
    [Produces("application/json")]
    public async Task<ActionResult> GetContent([FromRoute] Guid id)
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
    /// Request a download of a file. Not all files are available for download.
    /// On success, the raw bytes will be returned.
    /// </summary>
    /// <param name="id">Id of the file to download.</param>
    /// <returns>On success, the raw bytes will be returned.</returns>
    [HttpGet("files/{id:guid}/blob")]
    [ProducesResponseType<FileContentResult>(StatusCodes.Status200OK, "application/octet-stream")]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status501NotImplemented)]
    [Produces("application/octet-stream", "application/json", Type = typeof(FileContentResult))]
    public async Task<ActionResult> GetBlob(Guid id)
    {
        var file = await fileService.GetByIdAsync(id);
        var reference = file.References.FirstOrDefault(r => r.StorageType == StorageType.Db);
        if (reference == null) return BadRequest(
            new ErrorMessageDTO { Message = $"File {id} cannot be retrieved as blob from server."});
        if (!(reference is DbFileStorageReference dbRef)) return StatusCode(StatusCodes.Status500InternalServerError);

        if (reference.Algorithm == CompressionAlgorithm.Plain)
        {
            return File(dbRef.Data, file.FileType.MimeType ?? "application/octet-stream", 
                $"{file.Name}.{file.FileType.Abbreviation}");
        }

        return File(dbRef.Data, "application/octet-stream", $"{file.Name}.dat");
    }

    
    [HttpGet("file-refs")]
    [ProducesResponseType<FileSummaryDTO>(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<ActionResult> GetReferences([FromQuery] Guid? storeGuid, [FromQuery] string? type)
    {
        StorageType? storageType = null;
        if (type is not null)
        {
            if (!Enum.TryParse<StorageType>(type, out var result))
            {
                return BadRequest(new ErrorMessageDTO() { Message = $"Unknown storage type: '{type}'" });
            }
            storageType = result;
        }

        var refs = await fileService.GetStorageReferencesAsync(storeGuid, storageType);
        return Ok(refs.Select(referenceMapper.Export));
    }
}