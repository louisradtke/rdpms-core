using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister(registerFlags: RegisterFlags.ShallowInterfaces | RegisterFlags.Self)]
public class DataSetDetailedDTOMapper(FileSummaryDTOMapper fileMapper)
    : IExportMapper<DataSet, DataSetDetailedDTO>
{
    public DataSetDetailedDTO Export(DataSet domain)
    {
        var beginStamp = domain.Files.Min(f => f.BeginStamp);
        var endStamp = domain.Files.Max(f => f.EndStamp);
        return new DataSetDetailedDTO
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
            State = (DataSetStateDTO)domain.State,
            IsTimeSeries = domain.Files.Any(file => file.BeginStamp.HasValue),
            IsDeleted = domain.DeletedStamp.HasValue,
            MetadataFields = domain.MetadataJsonFields.Select(f => f.Key).ToList(),
            Files = domain.Files.Select(fileMapper.Export).ToList()
        };
    }
}
