namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Representing the type of a file according to
/// <see href="https://developer.mozilla.org/en-US/docs/Web/HTTP/MIME_types/Common_types">
/// MIME docs
/// </see>
/// </summary>
public record ContentTypeEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// the common file ending this file would have, lowercase, without leading dot (e.g. bag, png, json, ...)
    /// </summary>
    public string Abbreviation { get; set; } = string.Empty;

    /// <summary>
    /// The optional display name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description of the type, to reduce risk of confusion (e.g. multiple types of bin)
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// The optional MIME type, if overriding the list of known types
    /// </summary>
    public string? MimeType { get; set; }
    
    //TODO: Schemas
}
