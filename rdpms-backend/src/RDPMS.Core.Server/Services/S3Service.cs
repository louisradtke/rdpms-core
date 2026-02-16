using System.Collections.Concurrent;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public class S3Service(ISecretResolverService secretResolverService, ILogger<S3Service> logger) : IS3Service
{
    /// <summary>
    /// Cache of clients.
    /// </summary>
    private ConcurrentDictionary<Guid, IMinioClient> MinioClients { get; } = new();
    
    /// <summary>
    /// Tasks that check if clients are valid.
    /// They likely already terminated, so awaiting them may instantly yield a result.
    /// </summary>
    private ConcurrentDictionary<Guid, Task<bool>> ClientsValidated { get; } = new();

    /// <summary>
    /// Checks that a store is valid.
    /// Uses the <see cref="MinioClients"/> dict to access clients.
    /// </summary>
    /// <param name="store">The store representing S3 endpoint and bucket.</param>
    /// <returns>true, if bucket exists for client.
    /// false, if client was not found in dict or bucket does not exist.</returns>
    private async Task<bool> CheckStoreConnection(S3DataStore store)
    {
        if (!MinioClients.TryGetValue(store.Id, out var client))
        {
            return false;
        }
        var beArgs = new BucketExistsArgs()
            .WithBucket(store.Bucket);
        
        var result = await client.BucketExistsAsync(beArgs);
        if (!result) logger.LogDebug("Bucket {Bucket} does not exist for store {StoreId}.", store.Bucket, store.Id);
        return result;
    }
    
    /// <summary>
    /// Resolve a client for a store.
    /// </summary>
    /// <param name="store"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
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

        // if the client is not yet validated, start a task to validate it.
        // the returned task will thus either contain the existing check result or implicitly await the check.
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

    public async Task<byte[]> GetFileAsync(S3FileStorageReference reference, S3DataStore store)
    {
        var client = await ResolveClientAsync(store);

        var stream = new MemoryStream();
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(store.Bucket)
            .WithObject(store.KeyPrefix + reference.ObjectKey)
            .WithCallbackStream(s => s.CopyTo(stream));

        var stat = await client.GetObjectAsync(getObjectArgs);
        return stream.ToArray();
    }

    public async Task<bool> ValidateFileRefAsync(S3FileStorageReference reference, S3DataStore store)
    {
        var client = await ResolveClientAsync(store);
        var args = new StatObjectArgs()
            .WithBucket(store.Bucket)
            .WithObject(store.KeyPrefix + reference.ObjectKey);

        try
        {
            var obj = await client.StatObjectAsync(args);
            if (obj.Size == reference.SizeBytes)
            {
                logger.LogDebug("File {ObjectKey} in bucket {Bucket} for store {StoreId} validated successfully.",
                    reference.ObjectKey, store.Bucket, store.Id);
                return true;
            }

            logger.LogDebug("File {ObjectKey} in bucket {Bucket} for store {StoreId} has invalid size.",
                reference.ObjectKey, store.Bucket, store.Id);
            return false;
        }
        catch (ObjectNotFoundException onfe)
        {
            logger.LogDebug(onfe, "File {ObjectKey} in bucket {Bucket} for store {StoreId} not found.",
                reference.ObjectKey, store.Bucket, store.Id);
            return false;
        }
        catch (MinioException mex)
        {
            logger.LogDebug(mex, "Failed to stat object {ObjectKey} in bucket {Bucket} for store {StoreId}.",
                reference.ObjectKey, store.Bucket, store.Id);
            return false;
        }
    }

    public Task<IEnumerable<string>> ListAllFilesAsync(S3DataStore store)
    {
        throw new NotImplementedException();
    }
}