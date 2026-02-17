namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Request body for creating a new dataset.
/// Kept non-polymorphic on purpose to avoid discriminator/subtype binding issues.
/// </summary>
public record DataSetCreateRequestDTO
{
    /// <summary>
    /// Optional human-readable identifier.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Mandatory dataset name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Mandatory creation timestamp in UTC.
    /// </summary>
    public DateTime? CreatedStampUTC { get; set; }

    /// <summary>
    /// Mandatory parent collection id.
    /// </summary>
    public Guid? CollectionId { get; set; }
}
