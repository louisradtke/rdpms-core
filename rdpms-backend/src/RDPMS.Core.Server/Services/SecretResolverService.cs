namespace RDPMS.Core.Server.Services;

public class SecretResolverService : ISecretResolverService
{
    public string ResolveSecret(string secretUrl, Guid scope)
    {
        if (secretUrl.StartsWith("direct://"))
        {
            return secretUrl.Replace("direct://", "");
        }
        
        throw new ArgumentException("Invalid secret URL");
    }
}