using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Services;

public class ExecutionService(DbContext context) : IExecutionService
{
    private const string RegistrationLogSource = "rdpms.execution.registration";

    public async Task<ExecutionSummaryDTO> RegisterFinishedAsync(ExecutionFinishedRequestDTO request)
    {
        var sourceDatasetIds = DistinctIds(request.SourceDatasetIds);
        var outputDatasetIds = DistinctIds(request.OutputDatasetIds);
        var metadataIds = DistinctIds(request.MetadataIds);

        if (sourceDatasetIds.Count == 0)
        {
            throw new ArgumentException("At least one source dataset id is required.");
        }

        if (outputDatasetIds.Count == 0 && metadataIds.Count == 0)
        {
            throw new ArgumentException("At least one output dataset id or metadata id is required.");
        }

        var sourceDatasets = await LoadDatasets(sourceDatasetIds);
        var outputDatasets = await LoadDatasets(outputDatasetIds);
        var outputCollection = await ResolveOutputCollection(request.OutputCollectionId, sourceDatasets, outputDatasets);

        if (metadataIds.Count > 0)
        {
            await EnsureMetadataExists(metadataIds);
        }

        var now = DateTime.UtcNow;
        var terminatedStamp = request.TerminatedStampUtc ?? now;
        var job = new Job(NormalizeName(request.Name, request.PipelineKey))
        {
            State = JobState.Finished,
            CreatedStamp = now,
            StartedStamp = request.StartedStampUtc,
            TerminatedStamp = terminatedStamp,
            LastUpdateStamp = terminatedStamp,
            OutputCollectionEntity = outputCollection,
            SourceDatasets = sourceDatasets,
            OutputDatasets = outputDatasets,
            Logs =
            [
                new LogSection
                {
                    SourceName = RegistrationLogSource,
                    LogContent = JsonSerializer.Serialize(new
                    {
                        request.PipelineKey,
                        request.TrackerId,
                        MetadataIds = metadataIds,
                    }),
                }
            ]
        };

        await context.Set<Job>().AddAsync(job);
        await context.SaveChangesAsync();

        return MapSummary(job, sourceDatasetIds, outputDatasetIds, metadataIds, request.PipelineKey, request.TrackerId);
    }

    public async Task<List<ExecutionSummaryDTO>> GetAsync(Guid? ancestorOf = null, Guid? childOf = null)
    {
        var query = context.Set<Job>()
            .AsNoTracking()
            .Include(job => job.SourceDatasets)
            .Include(job => job.OutputDatasets)
            .Include(job => job.Logs)
            .AsQueryable();

        if (ancestorOf is not null)
        {
            query = query.Where(job => job.OutputDatasets.Any(dataset => dataset.Id == ancestorOf.Value));
        }

        if (childOf is not null)
        {
            query = query.Where(job => job.SourceDatasets.Any(dataset => dataset.Id == childOf.Value));
        }

        var jobs = await query
            .OrderByDescending(job => job.CreatedStamp)
            .ToListAsync();

        return jobs.Select(MapSummary).ToList();
    }

    public async Task<ExecutionSummaryDTO> GetByIdAsync(Guid id)
    {
        var job = await context.Set<Job>()
            .AsNoTracking()
            .Include(item => item.SourceDatasets)
            .Include(item => item.OutputDatasets)
            .Include(item => item.Logs)
            .SingleOrDefaultAsync(item => item.Id == id);

        return job is null
            ? throw new InvalidOperationException($"Execution {id} was not found.")
            : MapSummary(job);
    }

    private ExecutionSummaryDTO MapSummary(Job job)
    {
        var registration = ExtractRegistrationPayload(job);
        return MapSummary(
            job,
            job.SourceDatasets.Select(dataset => dataset.Id).Distinct().ToList(),
            job.OutputDatasets.Select(dataset => dataset.Id).Distinct().ToList(),
            registration.MetadataIds,
            registration.PipelineKey,
            registration.TrackerId);
    }

    private static ExecutionSummaryDTO MapSummary(
        Job job,
        List<Guid> sourceDatasetIds,
        List<Guid> outputDatasetIds,
        List<Guid> metadataIds,
        string? pipelineKey,
        string? trackerId)
    {
        return new ExecutionSummaryDTO
        {
            Id = job.Id,
            Name = job.Name,
            PipelineKey = pipelineKey,
            TrackerId = trackerId,
            State = job.State,
            CreatedStampUtc = job.CreatedStamp,
            StartedStampUtc = job.StartedStamp,
            TerminatedStampUtc = job.TerminatedStamp,
            SourceDatasetIds = sourceDatasetIds,
            OutputDatasetIds = outputDatasetIds,
            MetadataIds = metadataIds
        };
    }

    private static List<Guid> DistinctIds(IEnumerable<Guid>? ids)
    {
        return (ids ?? [])
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
    }

    private async Task<List<DataSet>> LoadDatasets(List<Guid> datasetIds)
    {
        if (datasetIds.Count == 0)
        {
            return [];
        }

        var datasets = await context.Set<DataSet>()
            .Include(ds => ds.ParentCollection)
            .Where(ds => datasetIds.Contains(ds.Id))
            .ToListAsync();

        var missingIds = datasetIds.Except(datasets.Select(ds => ds.Id)).ToList();
        if (missingIds.Count > 0)
        {
            throw new ArgumentException($"Unknown dataset id(s): {string.Join(", ", missingIds)}");
        }

        return datasets;
    }

    private async Task<DataCollectionEntity> ResolveOutputCollection(
        Guid? requestedOutputCollectionId,
        List<DataSet> sourceDatasets,
        List<DataSet> outputDatasets)
    {
        if (requestedOutputCollectionId is not null)
        {
            var collection = await context.Set<DataCollectionEntity>()
                .SingleOrDefaultAsync(c => c.Id == requestedOutputCollectionId.Value);
            return collection
                   ?? throw new ArgumentException($"Unknown output collection id: {requestedOutputCollectionId}");
        }

        var outputCollectionIds = outputDatasets
            .Select(ds => ds.ParentCollectionId)
            .Distinct()
            .ToList();
        if (outputCollectionIds.Count == 1)
        {
            return outputDatasets[0].ParentCollection
                   ?? await context.Set<DataCollectionEntity>().SingleAsync(c => c.Id == outputCollectionIds[0]);
        }

        if (outputCollectionIds.Count > 1)
        {
            throw new ArgumentException("All output datasets must belong to the same collection.");
        }

        var sourceCollectionIds = sourceDatasets
            .Select(ds => ds.ParentCollectionId)
            .Distinct()
            .ToList();
        if (sourceCollectionIds.Count == 1)
        {
            return sourceDatasets[0].ParentCollection
                   ?? await context.Set<DataCollectionEntity>().SingleAsync(c => c.Id == sourceCollectionIds[0]);
        }

        throw new ArgumentException("OutputCollectionId is required when no output dataset is provided.");
    }

    private async Task EnsureMetadataExists(List<Guid> metadataIds)
    {
        var existingIds = await context.Set<MetadataJsonField>()
            .Where(field => metadataIds.Contains(field.Id))
            .Select(field => field.Id)
            .ToListAsync();

        var missingIds = metadataIds.Except(existingIds).ToList();
        if (missingIds.Count > 0)
        {
            throw new ArgumentException($"Unknown metadata id(s): {string.Join(", ", missingIds)}");
        }
    }

    private static string NormalizeName(string? name, string? pipelineKey)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(pipelineKey))
        {
            return pipelineKey.Trim();
        }

        return "execution";
    }

    private static ExecutionRegistrationPayload ExtractRegistrationPayload(Job job)
    {
        foreach (var log in job.Logs)
        {
            if (!string.Equals(log.SourceName, RegistrationLogSource, StringComparison.Ordinal))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(log.LogContent))
            {
                continue;
            }

            try
            {
                var payload = JsonSerializer.Deserialize<ExecutionRegistrationPayload>(log.LogContent);
                if (payload is not null)
                {
                    payload.MetadataIds = DistinctIds(payload.MetadataIds);
                    return payload;
                }
            }
            catch (JsonException)
            {
                // Ignore malformed historical payloads and keep the read API working.
            }
        }

        return new ExecutionRegistrationPayload();
    }

    private sealed class ExecutionRegistrationPayload
    {
        public string? PipelineKey { get; set; }
        public string? TrackerId { get; set; }
        public List<Guid> MetadataIds { get; set; } = [];
    }
}
