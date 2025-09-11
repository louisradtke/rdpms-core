namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Class representing a managed data store, where the system can CRUD files.
/// </summary>
/// <param name="name">Display name of the store</param>
public abstract class DataStore(string name) : IUniqueEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Name of the data store
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// List of all files stored on this instance
    /// </summary>
    public List<DataFile> DataFiles { get; set; } = [];
    
    public StorageType StorageType { get; set; } = StorageType.S3;
}

/// <summary>
/// Represents a single storage bucket on some S3 instance.
/// </summary>
/// <param name="name"></param>
public class S3DataStore : DataStore
{
    public S3DataStore(string name) : base(name)
    {
        StorageType = StorageType.S3;
    }

    /// <summary>
    /// URL where the S3 instance is accessible. e.g., https://minio.example.com
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets prepended to all keys (file identifiers) in the bucket. e.g., "data/vehicle/"
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// The name of the bucket, where all keys are referring to.
    /// </summary>
    public string Bucket { get; set; } = string.Empty;
    
    /// <summary>
    /// Currently in the format "direct://your-access-key"
    /// </summary>
    public string? AccessKeyReference { get; set; }

    /// <summary>
    /// Currently in the format "direct://your-secret-key"
    /// </summary>
    public string? SecretKeyReference { get; set; }

    public bool UsePathStyle { get; set; } = true;            // MinIO commonly uses path-style
    public string? Region { get; set; }                        // optional for MinIO; required for S3
}
