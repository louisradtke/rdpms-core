namespace RDPMS.Core.Server.Model.DTO.V1;

public record ProjectSummaryDTO()
{
    /// <summary>
    /// The id of this project.
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// The name of this project.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// The slug of this project.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// All collections assigned to this project.
    /// </summary>
    public List<CollectionSummaryDTO>? Collections { get; set; }
    
    /// <summary>
    /// All data stores assigned to this project.
    /// </summary>
    public List<DataStoreSummaryDTO>? DataStores { get; set; }
}