namespace RDPMS.Core.Persistence.Model;

/// <summary>
/// Mapping type that associates jobs with the datasets they are using
/// </summary>
/// <param name="SourceDatasetId">Id of a dataset used for a job</param>
/// <param name="JobId">Id of the job, the dataset was used for</param>
public record DataSetUsedForJobsEntity(Guid SourceDatasetId, Guid JobId);
