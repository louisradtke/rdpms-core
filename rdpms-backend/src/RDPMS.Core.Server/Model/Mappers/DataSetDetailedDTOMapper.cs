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
        var beginStamp = domain.Files.Min(f => f.BeginStamp);
        var endStamp = domain.Files.Max(f => f.EndStamp);
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
            BeginStampUTC = beginStamp,
            EndStampUTC = endStamp,
            LifecycleState = domain.LifecycleState.ToString(),
            IsTimeSeries = domain.Files.Any(file => file.BeginStamp.HasValue),
            Files = domain.Files.Select(fileMapper.Export).ToList(),
            CollectionId = domain.ParentId,
            FileCount = domain.Files.Count,
            DeletionState = (DeletionStateDTO) (int) domain.DeletionState
        };
    }
}
