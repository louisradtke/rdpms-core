using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data/containers")]
[ApiVersion("1.0")]
public class ContainersController(IContainerService containerService, IStoreService storeService, ContainerSummaryDTOMapper cMapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ContainerSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContainerSummaryDTO>>> Get()
    {
        var list = await containerService.GetAllAsync();
        return Ok(list);
    }
    
    /// <summary>
    /// Add a single item to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] ContainerSummaryDTO dto)
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
        await containerService.AddAsync(cMapper.Import(dto, store));
        return Ok();
    }
}