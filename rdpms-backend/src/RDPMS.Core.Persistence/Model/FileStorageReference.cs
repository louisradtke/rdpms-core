using System.ComponentModel.DataAnnotations.Schema;

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
    public Guid Id { get; set; } = Guid.NewGuid();
    
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
    public StorageType StorageType { get; internal set; }

    public Guid? StoreFid { get; set; }
    public Guid? FileFid { get; set; }

    [NotMapped]
    public StorageAttributes Attributes
    {
        get => new() {Algorithm = Algorithm, SizeBytes = SizeBytes, SHA256Hash = SHA256Hash};
        set => (Algorithm, SizeBytes, SHA256Hash) = (value.Algorithm, value.SizeBytes, value.SHA256Hash);
    }
}

/// <summary>
/// Represents a reference to a file, available via S3.
/// </summary>
public class S3FileStorageReference : FileStorageReference
{
    public S3FileStorageReference()
    {
        StorageType = StorageType.S3;
    }

    public string ObjectKey { get; set; } = string.Empty;
}

/// <summary>
/// Represents a reference to a file, available without authentication.
/// </summary>
public class StaticFileStorageReference : FileStorageReference
{
    public StaticFileStorageReference()
    {
        StorageType = StorageType.Static;
    }
    
    public string URL { get; set; } = string.Empty;
}

/// <summary>
/// Represents a reference to a file, stored in the database.
/// </summary>
public class DbFileStorageReference : FileStorageReference
{
    public DbFileStorageReference()
    {
        StorageType = StorageType.Db;
    }

    public byte[] Data { get; set; } = [];
}

public class StorageAttributes
{
    public CompressionAlgorithm Algorithm { get; set; }
    public long SizeBytes { get; set; }
    public string SHA256Hash { get; set; } = string.Empty;
}
