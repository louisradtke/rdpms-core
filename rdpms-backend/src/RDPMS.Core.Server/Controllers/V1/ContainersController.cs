using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/containers")]
[ApiVersion("1.0")]
public class ContainersController(IContainerService containerService, IFileService fileService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<ContainerSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ContainerSummaryDTO>>> Get()
    {
        var list = await containerService.GetAllAsync();
        return Ok(list);
    }
}