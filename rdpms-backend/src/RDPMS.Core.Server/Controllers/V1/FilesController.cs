using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/files")]
[ApiVersion("1.0")]
public class FilesController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IEnumerable<FileSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<FileSummaryDTO>>> Get()
    {
        var dto = new FileSummaryDTO()
        {
            Name = "TestFilev1.txt",
            ContentType = new ContentTypeDTO
            {
                Abbreviation = ".txt",
                DisplayName = "txt",
                Description = "Text File"
            }
        };

        var list = new List<FileSummaryDTO> { dto };
        return Ok(list);
    }
    
    
}