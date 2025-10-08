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
    IDataCollectionEntityService collectionsService,
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
    public async Task<ActionResult<IEnumerable<ProjectSummaryDTO>>> GetAll()
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

    [HttpGet("{id}")]
    [ProducesResponseType<ProjectSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetSingle(string id)
    {
        Project? project;
        if (!Guid.TryParse(id, out var guid))
        {
            project = await slugService.ResolveProjectBySlugAsync(id);
            if (project == null)
            {
                return NotFound(new ErrorMessageDTO { Message = $"No project with slug {id} was found." });
            }
        }
        else
        {
            project = await projectService.GetByIdAsync(guid);
        }

        var slug = (await slugService.GetSlugsForEntityAsync<Project>(project.Id))
            .SingleOrDefault(s => s.State == SlugState.Active);

        var collectionCounts = await collectionsService.GetDatasetCounts(
            project.DataCollections.Select(c => c.Id));
        var collectionSlugs = await slugService
            .GetSlugsForEntitiesAsync<DataCollectionEntity>(project
                .DataCollections.Select(c => c.Id));
        var dto = peMapper.Export(project);
        dto.Slug = slug?.Value;
        foreach (var (cDto, count) in dto.Collections?.Zip(collectionCounts) ?? [])
        {
            cDto.DataSetCount = count;
            cDto.Slug = collectionSlugs[cDto.Id!.Value].SingleOrDefault(s => s.State == SlugState.Active)?.Value;
        }
        return Ok(dto);
    }

    /// <summary>
    /// Id field on the body is ignored.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Put([FromRoute] string id, [FromBody] ProjectSummaryDTO dto)
    {
        Project? project;
        if (Guid.TryParse(id, out var guid))
        {
            project = await projectService.GetByIdAsync(guid);
        }
        else
        {
            project = await slugService.ResolveProjectBySlugAsync(id);
            if (project == null)
            {
                return NotFound(new ErrorMessageDTO { Message = $"No project with slug {id} was found." });
            }

            guid = project.Id;
        }

        if (dto.Id is not null && dto.Id != guid)
            return BadRequest("Id in body must match route or be null.");

        if (string.IsNullOrWhiteSpace(dto.Name)) 
            return BadRequest("Name is required.");

        try
        {
            await projectService.UpdateNameAsync(guid, dto.Name);

            if (dto.Slug is null)
            {
                return NoContent();
            }

            var slugs = await slugService.GetSlugsForEntityAsync<Project>(guid);
            var currentSlug = slugs.SingleOrDefault(s => s.State == SlugState.Active);
            if (currentSlug?.Value != dto.Slug)
            {
                await slugService.RegisterSlugAsync(project, dto.Slug);
            }
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}