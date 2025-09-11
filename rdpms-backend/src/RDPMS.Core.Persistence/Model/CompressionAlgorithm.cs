namespace RDPMS.Core.Persistence.Model;

public enum CompressionAlgorithm
{
    Plain = 0,
    Bz2 = 1,
    Gz = 2,
    Xz = 3,
    ZStd = 4,
    Lz4 = 5
}