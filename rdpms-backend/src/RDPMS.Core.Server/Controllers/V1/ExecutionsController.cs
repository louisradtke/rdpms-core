using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/executions")]
[ApiVersion("1.0")]
public class ExecutionsController(IExecutionService executionService) : ControllerBase
{
    /// <summary>
    /// Register a finished execution and its direct source/output artifacts.
    /// </summary>
    [HttpPost("finished")]
    [Consumes("application/json")]
    [ProducesResponseType<ExecutionSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExecutionSummaryDTO>> RegisterFinished(
        [FromBody] ExecutionFinishedRequestDTO request)
    {
        try
        {
            var result = await executionService.RegisterFinishedAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }
    }

    /// <summary>
    /// Get all executions, optionally filtered by the dataset they produced or consumed.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ExecutionSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ExecutionSummaryDTO>>> Get(
        [FromQuery] Guid? ancestorOf = null,
        [FromQuery] Guid? childOf = null)
    {
        var result = await executionService.GetAsync(ancestorOf, childOf);
        return Ok(result);
    }

    /// <summary>
    /// Get a single execution by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ExecutionSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionSummaryDTO>> GetById([FromRoute] Guid id)
    {
        try
        {
            var result = await executionService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorMessageDTO { Message = ex.Message });
        }
    }
}
