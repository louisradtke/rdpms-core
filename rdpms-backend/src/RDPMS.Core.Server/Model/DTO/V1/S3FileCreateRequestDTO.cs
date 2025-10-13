namespace RDPMS.Core.Server.Model.DTO.V1;

public record S3FileCreateRequestDTO : FileCreateRequestDTO
{
    public string? CompressionAlgorithm { get; set; }

    /// <summary>
    /// If file is compressed, this is the SHA256 hash of the compressed file.
    /// </summary>
    public string? CompressedSHA256Hash { get; set; }
    
    public long? CompressedSizeBytes { get; set; }
}