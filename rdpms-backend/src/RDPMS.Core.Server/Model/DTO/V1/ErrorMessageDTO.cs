namespace RDPMS.Core.Server.Model.DTO.V1;

public class ErrorMessageDTO
{
    /// <summary>
    /// The error message. This must be set. If the "user friendly" <see cref="DisplayMessage"/> is set, this may
    /// become technical.
    /// </summary>
    public string? Message { get; set; }
    
    /// <summary>
    /// A message dedicated to the user. If null, <see cref="Message"/> is the fallback.
    /// </summary>
    public string? DisplayMessage { get; set; }
}