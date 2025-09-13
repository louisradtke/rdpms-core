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
    /// <summary>
    /// Get all data stores.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<DataStoreSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataStoreSummaryDTO>>> Get()
    {
        var list = await storeService.GetAllAsync();
        return Ok(list);
    }
    
    /// <summary>
    /// Get a single data store by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [ProducesResponseType<DataStoreSummaryDTO>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DataStoreSummaryDTO>> GetById(Guid id)
    {
        var item = await storeService.GetByIdAsync(id);
        return Ok(storeMapper.Export(item));
    }
}