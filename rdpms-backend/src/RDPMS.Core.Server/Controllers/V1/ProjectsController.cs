using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Consumes("application/json")]
[Route("api/v{version:apiVersion}/projects")]
[ApiVersion("1.0")]
public class ProjectsController(
    IProjectService projectService,
    IDataCollectionEntityService collectionsService,
    IFileService fileService,
    ISlugService slugService,
    IExportMapper<Project, ProjectSummaryDTO> peMapper,
    ILogger<CollectionsController> logger) : ControllerBase
{
    /// <summary>
    /// Get all projects.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ProjectSummaryDTO>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProjectSummaryDTO>>> GetAll([FromQuery] string? slug = null)
    {
        if (slug is not null && !SlugUtil.IsValidSlug(slug))
        {
            BadRequest(new ErrorMessageDTO { Message = "Slug is not valid." });
        }

        var domainProjects = await projectService.GetAllAsync();
        var query = domainProjects
            .AsQueryable();
        if (slug is not null)
        {
            query = query.Where(p => p.Slug == slug);
        }
        var dtos = query.AsEnumerable().Select(peMapper.Export).ToList();
        await EnrichStoreUsage(dtos.SelectMany(project => project.DataStores ?? []));
        return Ok(dtos);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProjectSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetSingle(Guid id)
    {
        var project = await projectService.GetByIdAsync(id);

        var collectionCounts = await collectionsService.GetDatasetCounts(
            project.DataCollections.Select(c => c.Id));
        var collectionSlugs = await slugService
            .GetSlugsForEntitiesAsync<DataCollectionEntity>(project
                .DataCollections.Select(c => c.Id));
        var dto = peMapper.Export(project);
        await EnrichStoreUsage(dto.DataStores);
        foreach (var cDto in dto.Collections ?? [])
        {
            cDto.DataSetCount = collectionCounts.GetValueOrDefault(cDto.Id!.Value, 0);
            cDto.Slug = collectionSlugs[cDto.Id!.Value].SingleOrDefault(s => s.State == SlugState.Active)?.Value;
        }
        return Ok(dto);
    }

    private async Task EnrichStoreUsage(IEnumerable<DataStoreSummaryDTO>? stores)
    {
        var storeList = stores?.ToList() ?? [];
        if (storeList.Count == 0)
        {
            return;
        }

        var storeIds = storeList
            .Where(store => store.Id is not null)
            .Select(store => store.Id!.Value)
            .ToHashSet();
        var refs = (await fileService.GetStorageReferencesAsync())
            .Where(reference => reference.StoreFid is not null && storeIds.Contains(reference.StoreFid.Value))
            .GroupBy(reference => reference.StoreFid!.Value)
            .ToDictionary(
                group => group.Key,
                group => new
                {
                    Count = group.Count(),
                    Bytes = group.Sum(reference => reference.SizeBytes)
                });

        foreach (var store in storeList)
        {
            if (store.Id is null || !refs.TryGetValue(store.Id.Value, out var usage))
            {
                store.FilesCount = 0;
                store.StorageReferenceCount = 0;
                store.StorageBytes = 0;
                continue;
            }

            store.FilesCount = usage.Count;
            store.StorageReferenceCount = usage.Count;
            store.StorageBytes = usage.Bytes;
        }
    }

    /// <summary>
    /// Id field on the body is ignored.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Put([FromRoute] Guid id, [FromBody] ProjectSummaryDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest("Name is required.");
        }

        try
        {
            await projectService.UpdateNameAndSlugAsync(id, dto.Name, dto.Slug);
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
