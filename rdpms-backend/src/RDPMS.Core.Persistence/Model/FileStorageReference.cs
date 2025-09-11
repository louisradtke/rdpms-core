namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Entity representing a single reference to the storage location of a <see cref="DataFile"/>, and whether and how it
/// is compressed.
/// </summary>
public abstract class FileStorageReference : IUniqueEntity
{
    /// <summary>
    /// Unique identifier of the storage reference.
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// The compression algorithm used to compress the file.
    /// </summary>
    public CompressionAlgorithm Algorithm { get; set; }

    /// <summary>
    /// Size after compression in bytes, if applicable. If not, this is the size of the uncompressed file.
    /// </summary>
    public long SizeBytes { get; set; }
    
    /// <summary>
    /// SHA256 hash of the file.
    /// </summary>
    public string SHA256Hash { get; set; } = string.Empty;
    
    /// <summary>
    /// The resource type (S3, Static, ...).
    /// </summary>
    public StorageType StorageType { get; set; }
}

/// <summary>
/// Represents a location to a file, available via S3.
/// </summary>
public class S3FileStorageReference : FileStorageReference
{
    public string Bucket { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string? ObjectVersionId { get; set; }
    public string? ETag { get; set; }
}

/// <summary>
/// Represents a location to a file, available without authentication.
/// </summary>
public class StaticFileStorageReference : FileStorageReference
{
    public string URL { get; set; } = string.Empty;
}
