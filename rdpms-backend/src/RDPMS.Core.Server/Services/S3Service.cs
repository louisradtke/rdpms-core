using Minio;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public class S3Service(ISecretResolverService secretResolverService)
{
    private Dictionary<Guid, IMinioClient> MinioClients { get; } = new();

    private IMinioClient ResolveClient(S3DataStore store)
    {
        lock (MinioClients)
        {
            if (MinioClients.TryGetValue(store.Id, out var existingClient))
            {
                return existingClient;
            }

            if (store.AccessKeyReference is null || store.SecretKeyReference is null)
            {
                throw new ArgumentException("Store has null credentials.");
            }

            var accessKey = secretResolverService.ResolveSecret(store.AccessKeyReference, store.Id);
            var secretKey = secretResolverService.ResolveSecret(store.SecretKeyReference, store.Id);
            var clientBuilder = new MinioClient()
                .WithEndpoint(store.EndpointAddress)
                .WithCredentials(accessKey, secretKey);
            if (store.Region is not null)
            {
                clientBuilder.WithRegion(store.Region);
            }
            if (store.IsSsl)
            {
                clientBuilder.WithSSL();
            }
            var client = clientBuilder.Build();
            MinioClients.Add(store.Id, client);
            return client;
        }
    }
}