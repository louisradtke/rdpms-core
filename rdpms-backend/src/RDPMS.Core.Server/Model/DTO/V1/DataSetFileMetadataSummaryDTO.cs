namespace RDPMS.Core.Server.Model.DTO.V1;

public record DataSetFileMetadataSummaryDTO : DataSetSummaryDTO
{
    public List<FileMetadataSummaryDTO>? Files { get; set; }

    public static DataSetFileMetadataSummaryDTO Create(DataSetSummaryDTO source)
    {
        return new DataSetFileMetadataSummaryDTO
        {
            Id = source.Id,
            Slug = source.Slug,
            Name = source.Name,
            AssignedTags = source.AssignedTags,
            CreatedStampUTC = source.CreatedStampUTC,
            DeletedStampUTC = source.DeletedStampUTC,
            BeginStampUTC = source.BeginStampUTC,
            EndStampUTC = source.EndStampUTC,
            LifecycleState = source.LifecycleState,
            DeletionState = source.DeletionState,
            IsTimeSeries = source.IsTimeSeries,
            FileCount = source.FileCount,
            CollectionId = source.CollectionId
        };
    }
}
