namespace RDPMS.Core.Persistence.Model;

public enum StorageType
{
    /// <summary>
    /// File is stored on a S3 bucket.
    /// </summary>
    S3 = 0,
    
    /// <summary>
    /// File is accessible via a URL, somewhere on the internet.
    /// </summary>
    Static = 1
}