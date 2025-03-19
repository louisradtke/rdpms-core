using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/data/stores")]
[ApiVersion("1.0")]
public class StoresController(IStoreService storeService, StoreSummaryDTOMapper storeMapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<DataStoreSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataStoreSummaryDTO>>> Get()
    {
        var list = await storeService.GetAllAsync();
        return Ok(list);
    }
    
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] DataStoreSummaryDTO dto)
    {
        if (dto.Id != null)
        {
            return BadRequest("Id is not allowed to be set.");
        }
        
        await storeService.AddAsync(storeMapper.Import(dto));
        return Ok();
    }

}