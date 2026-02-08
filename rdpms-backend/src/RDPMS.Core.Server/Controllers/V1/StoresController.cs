using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
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
    [ProducesResponseType<IEnumerable<ErrorMessageDTO>>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DataStoreSummaryDTO>>> Get([FromQuery] string? type = null,
        [FromQuery] Guid? parentProjectId = null)
    {
        var query = (await storeService.GetAllAsync())
            .AsQueryable();
        if (type is not null)
        {
            if (!Enum.TryParse<StorageType>(type, out var result))
            {
                return BadRequest(new ErrorMessageDTO { Message = $"Unknown storage type: '{type}'" });
            }
            query = query.Where(s => s.StorageType == result);
        }

        if (parentProjectId is not null)
        {
            if (parentProjectId == Guid.Empty)
            {
                return BadRequest(new ErrorMessageDTO { Message = "ParentProjectId cannot be empty." });
            }
            query = query.Where(s => s.ParentProjectId == parentProjectId);
        }
        
        return Ok(query.AsEnumerable().Select(storeMapper.Export));
    }
    
    /// <summary>
    /// Get a single data store by id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DataStoreSummaryDTO>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DataStoreSummaryDTO>> GetById(Guid id)
    {
        var item = await storeService.GetByIdAsync(id);
        return Ok(storeMapper.Export(item));
    }
}