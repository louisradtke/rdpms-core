using System.Collections.Concurrent;
using Minio;
using Minio.DataModel.Args;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public class S3Service(ISecretResolverService secretResolverService) : IS3Service
{
    private ConcurrentDictionary<Guid, IMinioClient> MinioClients { get; } = new();
    private ConcurrentDictionary<Guid, Task<bool>> ClientsValidated { get; } = new();

    private Task<bool> CheckStoreConnection(S3DataStore store)
    {
        if (!MinioClients.TryGetValue(store.Id, out var client))
        {
            return Task.FromResult(false);
        }
        var beArgs = new BucketExistsArgs()
            .WithBucket(store.Bucket);
        
        return client.BucketExistsAsync(beArgs);
    }
    
    private async Task<IMinioClient> ResolveClientAsync(S3DataStore store)
    {
        IMinioClient? client;
        lock (MinioClients)
        {
            if (MinioClients.TryGetValue(store.Id, out client))
            {
                return client;
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
            client = clientBuilder.Build();

            MinioClients.TryAdd(store.Id, client);
        }

        var checkTask = ClientsValidated.GetOrAdd(
            store.Id,
            static (_, arg) => Task.Run(() => arg.Item2.CheckStoreConnection(arg.Item1)),
            new Tuple<S3DataStore, S3Service>(store, this));

        if (!await checkTask)
        {
            throw new ArgumentException("Store check failed.");
        }

        return client;
    }

    public async Task<string> RequestPresignedUploadUrlAsync(S3DataStore store, string key)
    {
        var client = await ResolveClientAsync(store);

        var presignedArgs = new PresignedPutObjectArgs()
            .WithBucket(store.Bucket)
            .WithObject(store.KeyPrefix + key)
            .WithExpiry(3600); // 1 hour expiry
        
        return await client.PresignedPutObjectAsync(presignedArgs);
    }

    public async Task<string> RequestPresignedDownloadUrlAsync(S3FileStorageReference reference, S3DataStore store)
    {
        var client = await ResolveClientAsync(store);

        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(store.Bucket)
            .WithObject(store.KeyPrefix + reference.ObjectKey)
            .WithExpiry(3600); // 1 hour expiry
        
        return await client.PresignedGetObjectAsync(presignedArgs);
    }

    public Task<IEnumerable<string>> ListAllFilesAsync(S3DataStore store)
    {
        throw new NotImplementedException();
    }
}