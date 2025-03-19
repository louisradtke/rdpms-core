namespace RDPMS.Core.Server;

public abstract class CollectableError
{
    private const int PreviewLength = 20;
    public ErrorSeverity Severity { get; protected set;}

    public abstract string GetMessage();

    public override string ToString()
    {
        var type = GetType().Name;
        var msg = GetMessage();
        if (string.IsNullOrWhiteSpace(msg)) return $"{type}(Severity={Severity.ToString()})";
        
        var preview = msg[..Math.Min(msg.Length, PreviewLength)];
        if (msg.Length > PreviewLength) preview += " ...";
        return $"{type}(Severity={Severity.ToString()}, Message={preview})";
    }
}