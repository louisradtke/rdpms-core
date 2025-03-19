namespace RDPMS.Core.Server;

public class SimpleCollectableError : CollectableError
{
    public string Message { get; set; }
    public SimpleCollectableError(string message, ErrorSeverity severity)
    {
        Message = message;
        Severity = severity;
    }
    
    public override string GetMessage() => Message;
}