using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister(registerFlags: RegisterFlags.ShallowInterfaces | RegisterFlags.Self)]
public class DataSetDetailedDTOMapper(FileSummaryDTOMapper fileMapper)
    : IExportMapper<DataSet, DataSetSummaryDTO>
{
    public DataSetSummaryDTO Export(DataSet domain)
    {
        return new DataSetSummaryDTO
        {
            Id = domain.Id,
            Slug = domain.Slug,
            Name = domain.Name,
            AssignedTags = domain.AssignedTags.Select(tag => new TagDTO
            {
                Id = tag.Id,
                Name = tag.Name,
            }).ToList(),
            CreatedStampUTC = domain.CreatedStamp,
            DeletedStampUTC = domain.DeletedStamp,
            BeginStampUTC = domain.BeginStamp,
            EndStampUTC = domain.EndStamp,
            LifecycleState = domain.LifecycleState.ToString(),
            IsTimeSeries = domain.IsTimeSeries,
            Files = domain.Files.Select(fileMapper.Export).ToList(),
            CollectionId = domain.ParentId,
            FileCount = domain.Files.Count,
            TotalSizeBytes = domain.Files.Sum(file => file.SizeBytes),
            DeletionState = (DeletionStateDTO) (int) domain.DeletionState
        };
    }
}
