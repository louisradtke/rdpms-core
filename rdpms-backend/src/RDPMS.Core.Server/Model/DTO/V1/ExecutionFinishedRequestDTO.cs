namespace RDPMS.Core.Server.Model.DTO.V1;

public record ExecutionFinishedRequestDTO
{
    public string? Name { get; set; }
    public string? PipelineKey { get; set; }
    public string? TrackerId { get; set; }
    public List<Guid>? SourceDatasetIds { get; set; }
    public List<Guid>? OutputDatasetIds { get; set; }
    public List<Guid>? MetadataIds { get; set; }
    public Guid? OutputCollectionId { get; set; }
    public DateTime? StartedStampUtc { get; set; }
    public DateTime? TerminatedStampUtc { get; set; }
}
