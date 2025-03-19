using System.Collections.ObjectModel;
using System.Text;

namespace RDPMS.Core.Server;

public class ErrorCollection : Collection<CollectableError>
{
    public bool ContainsError() => this.Any(e => e.Severity >= ErrorSeverity.Error);
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var err in this)   
        {
            sb.AppendLine(err.ToString());
        }
        return sb.ToString();
    }
}