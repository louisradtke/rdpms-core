namespace RDPMS.Core.Server.Model.DTO.V1;

public record FileMetadataSummaryDTO : FileSummaryDTO
{
    public static FileMetadataSummaryDTO Create(FileSummaryDTO source)
    {
        return new FileMetadataSummaryDTO
        {
            Id = source.Id,
            Name = source.Name,
            DownloadURI = source.DownloadURI,
            ContentType = source.ContentType,
            Size = source.Size,
            CreatedStampUTC = source.CreatedStampUTC,
            DeletedStampUTC = source.DeletedStampUTC,
            BeginStampUTC = source.BeginStampUTC,
            EndStampUTC = source.EndStampUTC,
            IsTimeSeries = source.IsTimeSeries,
            DeletionState = source.DeletionState
        };
    }
}
