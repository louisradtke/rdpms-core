using RDPMS.Core.Infra.AppInitialization;

namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileStorageReferenceSummaryDTO
{
    public Guid? Id { get; set; }
    public string? CompressionAlgorithm { get; set; }
    public long? SizeBytes { get; set; }
    public string? SHA256Hash { get; set; }
    public string? StorageType { get; set; }
}