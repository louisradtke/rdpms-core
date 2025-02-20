namespace RDPMS.Core.Persistence.Model;

public record PipelineInstanceEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Instance-wide pipeline identifier. Counter is managed by database.
    /// </summary>
    public UInt64 LocalId { get; set; }

    /// <summary>
    /// All job instances associated with this pipeline
    /// </summary>
    public List<JobEntityEntity> Jobs { get; init; } = [];
}
