using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class FileSummaryDTOMapper(ContentTypeDTOMapper ctMapper)
    : IExportMapper<DataFile, FileSummaryDTO>
{ 
    public FileSummaryDTO Export(DataFile domain)
    {
        var contentTypeDTO = ctMapper.Export(domain.FileType);

        return new FileSummaryDTO
        {
            Id = domain.Id,
            Name = domain.Name,
            ContentType = contentTypeDTO,
            Size = domain.SizeBytes,
            CreatedStampUTC = domain.CreatedStamp,
            DeletedStampUTC = domain.DeletedStamp,
            BeginStampUTC = domain.BeginStamp,
            EndStampUTC = domain.EndStamp,
            IsTimeSeries = domain.IsTimeSeries,
            DeletionState = (DeletionStateDTO) (int) domain.DeletionState
        };
    }
}