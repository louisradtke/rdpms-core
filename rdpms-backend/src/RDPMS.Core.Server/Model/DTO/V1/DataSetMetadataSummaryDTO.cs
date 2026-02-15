namespace RDPMS.Core.Server.Model.DTO.V1;

public record DataSetMetadataSummaryDTO : DataSetSummaryDTO
{
    public static DataSetMetadataSummaryDTO Create(DataSetSummaryDTO source)
    {
        return new DataSetMetadataSummaryDTO
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
