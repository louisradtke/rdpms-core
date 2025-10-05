using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/projects")]
[ApiVersion("1.0")]
public class ProjectsController(
    IProjectService projectService,
    ISlugService slugService,
    IExportMapper<Project, ProjectSummaryDTO> peMapper,
    ILogger<CollectionsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all projects.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ProjectSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProjectSummaryDTO>>> Get()
    {
        var domainProjects = await projectService.GetAllAsync();
        var list = domainProjects.Select(peMapper.Export).ToList();
        foreach (var project in list)
        {
            var slugsForEntity = await slugService.GetSlugsForEntityAsync<Project>(project.Id!.Value);
            project.Slug = slugsForEntity.FirstOrDefault(s => s.State == SlugState.Active)?.Value;
        }
        return Ok(list);
    }
}