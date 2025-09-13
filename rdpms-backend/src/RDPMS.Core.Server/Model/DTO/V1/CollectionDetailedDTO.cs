namespace RDPMS.Core.Server.Model.DTO.V1;

public record CollectionDetailedDTO : CollectionSummaryDTO
{
    public List<DataSetSummaryDTO> DataSets { get; set; } = [];
}