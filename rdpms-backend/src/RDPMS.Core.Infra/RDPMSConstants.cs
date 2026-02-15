namespace RDPMS.Core.Infra;

public static class RDPMSConstants
{
    public static readonly Guid GlobalProjectId = Guid.Parse("11F819A0-6857-4A9F-8A77-CAF1A845776E");
    // public static readonly Guid GlobalLabelId = Guid.Parse("11F819A0-6857-4A9F-8A77-CAF1A845776E");
    public static readonly Guid DummyS3StoreId = Guid.Parse("F9D3A025-6382-4865-834A-8B486CA5F1B1");
    public static readonly Guid DummyDataCollectionId = Guid.Parse("c6f8fabb-911f-4cfe-983e-893497584241");
    public static readonly Guid IngressDataCollectionId = Guid.Parse("aeef3623-149c-4f81-837e-5d1354d3f9ca");
    public static readonly Guid DataPreviewsCollectionId = Guid.Parse("2c43b963-6fb9-4e11-894e-4d37f05ce515");
    public static readonly long MaxDbFileSize = 100 * 1024; // 100 KB
}
