using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public class FileCreateResponseDTOMapper
{
    public static FileCreateResponseDTO ToDTO(FileUploadTarget domain)
    {
        return new FileCreateResponseDTO
        {
            UploadUri = domain.Uri.ToString(),
            FileId = domain.FileId
        };
    } 
}