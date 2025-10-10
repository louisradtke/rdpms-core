using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class FileStorageReferenceSummaryDTOMapper : IExportMapper<FileStorageReference, FileStorageReferenceSummaryDTO>
{ 
    public FileStorageReferenceSummaryDTO Export(FileStorageReference domain)
    {
        return new FileStorageReferenceSummaryDTO
        {
            Id = domain.Id,
            SizeBytes = domain.SizeBytes,
            SHA256Hash = domain.SHA256Hash,
            StorageType = domain.StorageType.ToString().ToLower()
        };
    }
}