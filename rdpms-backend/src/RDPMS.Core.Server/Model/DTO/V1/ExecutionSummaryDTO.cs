using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Model.DTO.V1;

public record ExecutionSummaryDTO
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? PipelineKey { get; set; }
    public string? TrackerId { get; set; }
    public JobState State { get; set; }
    public DateTime CreatedStampUtc { get; set; }
    public DateTime? StartedStampUtc { get; set; }
    public DateTime? TerminatedStampUtc { get; set; }
    public List<Guid> SourceDatasetIds { get; set; } = [];
    public List<Guid> OutputDatasetIds { get; set; } = [];
    public List<Guid> MetadataIds { get; set; } = [];
}
