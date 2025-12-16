using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Class representing a managed data store, where the system can CRUD files.
/// </summary>
/// <param name="name">Display name of the store</param>
public abstract class DataStore(string name) : IUniqueEntity, IUniqueEntityWithSlugAndParent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ParentProjectId { get; set; }
    public Project? ParentProject { get; set; }

    /// <summary>
    /// Id of the parent <see cref="Project"/>. Nullability is a convenience feature, every store should have a parent
    /// project.
    /// </summary>
    [NotMapped]
    public Guid? ParentId => ParentProjectId;

    public string? Slug { get; set; }

    /// <summary>
    /// Name of the data store
    /// </summary>
    public string Name { get; set; } = name;

    public string? Description { get; set; }

    public StorageType StorageType { get; set; } = StorageType.S3;

    public abstract string GetPublicInfoContentJson();
}

/// <summary>
/// Represents a single storage bucket on some S3 instance.
/// </summary>
public class S3DataStore : DataStore
{
    public static readonly Regex UrlRegex = new(@"^http(s?)://[\w\-\.]+(:[0-9]{1,5})?$");
    private string _endpointUrl = string.Empty;

    public S3DataStore(string name) : base(name)
    {
        StorageType = StorageType.S3;
    }

    /// <summary>
    /// URL where the S3 instance is accessible. e.g., https://minio.example.com
    /// </summary>
    public string EndpointUrl
    {
        get => _endpointUrl;
        set {
            if (!UrlRegex.IsMatch(value)) throw new ArgumentException("Invalid URL");
            _endpointUrl = value;
        }
    }

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

    public override string GetPublicInfoContentJson()
    {
        return JsonConvert.SerializeObject(new Dictionary<string, object>()
        {
            { "endpointUrl", EndpointUrl },
            { "keyPrefix", KeyPrefix },
            { "bucket", Bucket }
        });
    }
    
    public bool IsSsl => EndpointUrl.StartsWith("https://");
    
    public string EndpointAddress => IsSsl ? EndpointUrl.Replace("https://", "") : EndpointUrl.Replace("http://", "");
}
