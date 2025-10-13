namespace RDPMS.Core.Server.Services;

public interface ISecretResolverService
{
    /// <summary>
    /// Resolves a secret from a URL, given the scope for access restriction.
    /// </summary>
    /// <param name="secretUrl">Url in the format e.g. direct://$SECRET</param>
    /// <param name="scope">Id of the object, that the secret belongs to.</param>
    /// <returns>The resolved secret</returns>
    /// <exception cref="ArgumentException">Thrown, if the secret URL could not be parse.</exception>
    public string ResolveSecret(string secretUrl, Guid scope);
}