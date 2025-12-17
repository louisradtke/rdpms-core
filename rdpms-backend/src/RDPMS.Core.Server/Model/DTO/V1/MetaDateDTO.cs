namespace RDPMS.Core.Server.Model.DTO.V1;

public record MetaDateDTO()
{
    /// <summary>
    /// The id of this meta date.
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// List of schema ids that have been validated against this meta date.
    /// </summary>
    public List<string>? ValidatedSchemaIds { get; set; }
    
    /// <summary>
    /// The id of the file representing the stored meta date. Retrieve content via Files API.
    /// </summary>
    public Guid? FileId { get; set; }
    
    /// <summary>
    /// The content type of the file representing the stored meta date.
    /// </summary>
    public ContentTypeDTO? FileContentType { get; set; }
}
