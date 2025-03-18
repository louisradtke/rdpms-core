namespace RDPMS.Core.Server.Model.DTO.V1;

public class ContainerSummaryDTO
{
    public Guid? Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public int? DataFilesCount { get; set; } = 0;
    
    //TODO: data store summary
}