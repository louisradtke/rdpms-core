using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class DataFileCreateRequestDTOMapper
{
    public static DataFile ToDomain(DataFileCreateRequestDTO dto, ContentType fileType)
    {
        if (dto.Name == null || dto.Size == null || dto.CreatedStamp == null || dto.Hash == null)
        {
            throw new ArgumentException(
                "Name, Size, CreatedStamp, and Hash are required fields in DataFileCreateRequestDTO");
        }
        
        if ((dto.BeginStamp == null) != (dto.EndStamp == null))
        {
            throw new ArgumentException("BeginStamp and EndStamp must be both null or both non-null.");
        }

        return new DataFile
        {
            Name = dto.Name,
            FileType = fileType, // Assuming FileType is passed as a reference (e.g., from a lookup table or service).
            Size = dto.Size.Value,
            Hash = dto.Hash,
            CreatedStamp = dto.CreatedStamp.Value,
            BeginStamp = dto.BeginStamp,
            EndStamp = dto.EndStamp
        };
    }
}