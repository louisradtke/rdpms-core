namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Represents a summary of a dataset, including identifying information, timestamps, state, tags,
/// and metadata fields.
/// </summary>
public record DataSetDetailedDTO : DataSetSummaryDTO
{
    public List<FileSummaryDTO> Files { get; set; } = [];
}