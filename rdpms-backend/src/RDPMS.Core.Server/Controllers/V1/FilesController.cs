using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
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
    IMetadataService metadataService,
    IExportMapper<MetadataJsonField, MetaDateDTO> metadataMapper,
    FileSummaryDTOMapper fileMapper,
    IExportMapper<FileStorageReference, FileStorageReferenceSummaryDTO> referenceMapper,
    FileCreateRequestDTOMapper dfCreateReqMapper,
    LinkGenerator linkGenerator,
    ILogger<FilesController> logger) : ControllerBase
{
    /// <summary>
    /// Get files.
    /// </summary>
    /// <returns></returns>
    [HttpGet("files")]
    [ProducesResponseType<IEnumerable<FileSummaryDTO>>(StatusCodes.Status200OK)]
    [Produces("application/json")]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> Get(
        [FromQuery] FileListViewMode view = FileListViewMode.Summary)
    {
        var files = (await fileService.GetAllAsync()).ToList();
        if (view == FileListViewMode.Summary)
        {
            var list = files.Select(fileMapper.Export).ToList();
            foreach (var file in list)
            {
                if (file.Id == null) throw new IllegalStateException("File has null id");
                file.DownloadURI = fileService.GetContentApiUri(file.Id.Value, HttpContext);
            }
            return Ok(list);
        }

        var validatedMetaDates = await fileService.GetValidatedMetadates(
            files.Select(f => f.Id).ToList());
        var filesWithMetadata = files.Select(file =>
        {
            validatedMetaDates.TryGetValue(file.Id, out var list);
            var dto = fileMapper.Export(file);
            dto.DownloadURI = fileService.GetContentApiUri(file.Id, HttpContext);
            dto.MetaDates = file.MetadataJsonFields
                .Select(f => new AssignedMetaDateDTO
                {
                    MetadataKey = f.MetadataKey,
                    MetadataId = f.FieldId,
                    CollectionSchemaVerified = list?.Contains(f.MetadataKey) ?? false
                })
                .ToList();
            return dto;
        }).ToList();

        return Ok(filesWithMetadata);
    }

    private async Task<FileSummaryDTO> QueryFileDetailedDTO(Guid id)
    {
        var file = await fileService.GetByIdAsync(id);
        var fileDto = fileMapper.Export(file);
        fileDto.DownloadURI = fileService.GetContentApiUri(id, HttpContext);
        fileDto.References = file.References.Select(referenceMapper.Export).ToList();
        fileDto.Size = file.SizeBytes;

        var validatedMetaDates = await fileService.GetValidatedMetadates([file.Id]);
        validatedMetaDates.TryGetValue(file.Id, out var list);

        fileDto.MetaDates = file.MetadataJsonFields
            .Select(f => new AssignedMetaDateDTO
            {
                MetadataKey = f.MetadataKey,
                MetadataId = f.FieldId,
                CollectionSchemaVerified = list?.Contains(f.MetadataKey) ?? false
            })
            .ToList();

        return fileDto;
    }

    /// <summary>
    /// Get details of a single file.
    /// </summary>
    /// <param name="id">Id of the file.</param>
    /// <returns></returns>
    [HttpGet("files/{id:guid}")]
    [ProducesResponseType<FileSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Produces("application/json")]
    public async Task<ActionResult<FileSummaryDTO>> GetById(Guid id)
    {
        try
        {
            return Ok(await QueryFileDetailedDTO(id));
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

    /// <summary>
    /// Adds or sets meta documents for a file.
    /// </summary>
    /// <param name="id">ID of the file</param>
    /// <param name="key">Case-insensitive key of the meta date on the file</param>
    /// <param name="value">JSON meta document</param>
    /// <returns>HTTP 200 if meta date was replaced successfully (key refers to existing date).
    /// HTTP 201, if a new meta date was created.
    /// HTTP 400, if an input-related problem occurs.</returns>
    [HttpPut("files/{id:guid}/metadata/{key}")]
    [ProducesResponseType<MetaDateDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<MetaDateDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [Consumes("application/octet-stream", "application/json")]
    public async Task<ActionResult> AssignMetadate([FromRoute] Guid id, [FromRoute] string key,
        [FromBody] byte[] value)
    {
        DataFile file;
        try
        {
            file = await fileService.GetByIdAsync(id);
        }
        catch (InvalidOperationException)
        {
            return NotFound(new ErrorMessageDTO { Message = "No file with that id." });
        }

        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug." });

        var contentType = await typeService.GetByMimeType(
            "application/json",
            file.ParentDataSet?.ParentCollection?.ParentId
        );

        var valueStr = Encoding.UTF8.GetString(value);
        var field = await metadataService.MakeFieldFromValue(valueStr, contentType);
        var modified = file.MetadataJsonFields.Any(f =>
            f.MetadataKey.Equals(key, StringComparison.OrdinalIgnoreCase));

        await metadataService.AssignMetadate(file, key, field);

        var returnDto = metadataMapper.Export(await metadataService.GetByIdAsync(field.Id));

        if (!modified) return CreatedAtAction(nameof(MetaDataController.GetMetadate), "MetaData",
            new { field.Id }, returnDto);
        return Ok(returnDto);
    }

    /// <summary>
    /// Rename key for metadate relation.
    /// </summary>
    /// <param name="id">The file ID.</param>
    /// <param name="key">The old key.</param>
    /// <param name="newKey">The new key.</param>
    /// <returns></returns>
    [HttpPost("files/{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RenameMetadate([FromRoute] Guid id, [FromRoute] string key,
        [FromQuery] string newKey)
    {
        DataFile file;
        try
        {
            file = await fileService.GetByIdAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorMessageDTO { Message = ex.Message });
        }

        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for key." });
        if (!SlugUtil.IsValidSlug(newKey))
        {
            return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for newName." });
        }

        var normalizedKey = key.ToLowerInvariant();
        var normalizedNewKey = newKey.ToLowerInvariant();
        var field = file.MetadataJsonFields
            .SingleOrDefault(f =>
                f.MetadataKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));
        if (field == null) return BadRequest(new ErrorMessageDTO { Message = "No such metadata key." });

        field.MetadataKey = normalizedNewKey;
        await fileService.UpdateAsync(file);
        return Ok();
    }

    /// <summary>
    /// Removes metadate relation with resp. key.
    /// </summary>
    /// <param name="id">ID of the file.</param>
    /// <param name="key">Key to remove the relation for</param>
    /// <returns></returns>
    [HttpDelete("files/{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveMetadate([FromRoute] Guid id, [FromRoute] string key)
    {
        DataFile file;
        try
        {
            file = await fileService.GetByIdAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }

        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for key." });

        var removed = file.MetadataJsonFields.RemoveAll(f =>
            f.MetadataKey.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (removed == 0) return BadRequest(new ErrorMessageDTO { Message = "No such metadata key." });
        await fileService.UpdateAsync(file);
        return Ok();
    }
}
