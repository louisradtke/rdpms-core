using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/content-types")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class ContentTypeController(IContentTypeService typeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ContentTypeDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContentTypeDTO>>> Get()
    {
        var list = await typeService.GetAllAsync();
        return Ok(list.Select(ContentTypeDTOMapper.ToDTO));
    }

    /// <summary>
    /// Add a batch of content types to the system.
    /// </summary>
    /// <param name="dtos"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult> Post([FromBody] IEnumerable<ContentTypeDTO> dtos)
    {
        await typeService.AddRangeAsync(dtos.Select(ContentTypeDTOMapper.ToDomain));
        return Ok();
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ContentTypeDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContentTypeDTO>> GetSpecific([FromRoute] Guid id)
    {
        if (!await typeService.CheckForIdAsync(id))
        {
            return NotFound("no type with that id");
        }
        return Ok(await typeService.GetByIdAsync(id));
    }
    
}