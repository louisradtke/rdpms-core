using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class DataFileCreateResponseDTOMapper
{
    public static DataFileCreateResponseDTO ToDTO(FileUploadTarget domain)
    {
        return new DataFileCreateResponseDTO
        {
            UploadUri = domain.Uri.ToString(),
            FileId = domain.FileId
        };
    } 
}