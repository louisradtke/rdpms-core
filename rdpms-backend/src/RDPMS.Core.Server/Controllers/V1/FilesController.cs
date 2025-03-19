using Asp.Versioning;
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
    FileCreateRequestDTOMapper dfCreateReqMapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<FileSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> Get()
    {
        var list = await fileService.GetAllAsync();
        return Ok(list);
    }

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
    
}