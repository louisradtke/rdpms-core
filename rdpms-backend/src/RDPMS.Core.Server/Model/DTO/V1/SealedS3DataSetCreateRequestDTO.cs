using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Model.DTO.V1;

/// <summary>
/// Request body for registering an already existing S3-backed dataset and sealing it in one operation.
/// Object keys are relative to the referenced <see cref="S3DataStore.KeyPrefix"/>.
/// </summary>
public record SealedS3DataSetCreateRequestDTO
{
    /// <summary>
    /// Optional human-readable identifier. Must be unique within the collection.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Mandatory dataset name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Mandatory creation timestamp in UTC.
    /// </summary>
    public DateTime? CreatedStampUTC { get; set; }

    /// <summary>
    /// Mandatory parent collection id.
    /// </summary>
    public Guid? CollectionId { get; set; }

    /// <summary>
    /// Mandatory S3 datastore id. Its endpoint, bucket, and key prefix are used to validate all file references.
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// Files to register as part of this dataset.
    /// </summary>
    public List<SealedS3DataSetFileCreateRequestDTO>? Files { get; set; }
}

/// <summary>
/// File entry for <see cref="SealedS3DataSetCreateRequestDTO"/>.
/// </summary>
public record SealedS3DataSetFileCreateRequestDTO
{
    /// <summary>
    /// File name as stored in RDPMS. This may include a relative path inside the dataset.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// S3 object key relative to the datastore prefix.
    /// </summary>
    public string? ObjectKey { get; set; }

    /// <summary>
    /// Content type id for this file.
    /// </summary>
    public Guid? ContentTypeId { get; set; }

    /// <summary>
    /// Plain file size. For already existing S3 objects this is also used as the expected storage-reference size.
    /// </summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Optional SHA256 of the plain file. Unknown hashes may be omitted.
    /// </summary>
    public string? PlainSHA256Hash { get; set; }

    /// <summary>
    /// Optional SHA256 of the stored object. Defaults to PlainSHA256Hash when omitted.
    /// </summary>
    public string? StoredSHA256Hash { get; set; }

    /// <summary>
    /// Optional storage compression algorithm. Defaults to Plain.
    /// </summary>
    public string? CompressionAlgorithm { get; set; }

    /// <summary>
    /// Optional file creation timestamp in UTC. Defaults to dataset CreatedStampUTC.
    /// </summary>
    public DateTime? CreatedStampUTC { get; set; }

    /// <summary>
    /// Optional first timestamp represented by this file.
    /// </summary>
    public DateTime? BeginStampUTC { get; set; }

    /// <summary>
    /// Optional final timestamp represented by this file.
    /// </summary>
    public DateTime? EndStampUTC { get; set; }
}
