namespace RDPMS.Core.Server.Model.DTO.V1;

public record SchemaValidationResultDTO()
{
    /// <summary>
    /// Wether validation of the meta date was succesful or not.
    /// </summary>
    public bool? Succesful { get; set; }
    
    /// <summary>
    /// If validation failed, a list of reasons why.
    /// </summary>
    public List<string>? Reasons { get; set; }

    /// <summary>
    /// Optional validation trace output, intended for debugging.
    /// </summary>
    public List<string>? Traces { get; set; }
}
