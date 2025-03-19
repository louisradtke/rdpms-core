using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data/content-types")]
[ApiVersion("1.0")]
[Produces("application/json")]
public class ContentTypesController(IContentTypeService typeService, ContentTypeDTOMapper ctMapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ContentTypeDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContentTypeDTO>>> Get()
    {
        var list = await typeService.GetAllAsync();
        return Ok(list.Select(ctMapper.Export));
    }

    /// <summary>
    /// Add a single content type to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Post([FromBody] ContentTypeDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await typeService.AddAsync(ctMapper.Import(dto));
        return Ok();
    }
    
    /// <summary>
    /// Add a batch of content types to the system.
    /// </summary>
    /// <param name="dtos"></param>
    /// <returns></returns>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult> PostBatch([FromBody] IEnumerable<ContentTypeDTO> dtos)
    {
        var dtosList = dtos.ToList();
        if (dtosList.Any(d => d.Id != null))
        {
            return BadRequest("Id is not allowed to be set.");
        }

        await typeService.AddRangeAsync(dtosList.Select(ctMapper.Import));
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