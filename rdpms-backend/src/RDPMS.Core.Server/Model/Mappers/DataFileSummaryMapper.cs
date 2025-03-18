using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class DataFileSummaryMapper
{
    /// <summary>
    /// Maps a <see cref="DataFile"/> to a <see cref="DataFileSummaryDTO"/>.
    /// </summary>
    /// <param name="dataFile">The DataFile instance to map.</param>
    /// <returns>A mapped DataFileSummaryDTO instance.</returns>
    public static DataFileSummaryDTO ToDTO(DataFile dataFile)
    {
        return new DataFileSummaryDTO
        {
            Id = dataFile.Id,
            Name = dataFile.Name,
            ContentType = ContentTypeDTOMapper.ToDTO(dataFile.FileType),
            Size = dataFile.Size, // Assuming size is unavailable in DataFile
            CreatedStampUTC = dataFile.CreatedStamp,
            DeletedStampUTC = dataFile.DeletedStamp,
            BeginStampUTC = dataFile.BeginStamp,
            EndStampUTC = dataFile.EndStamp,
            IsTimeSeries = dataFile.IsTimeSeries,
            IsDeleted = dataFile.IsDeleted
        };
    }

    // /// <summary>
    // /// Maps a <see cref="DataFileSummaryDTO"/> to a <see cref="DataFile"/>.
    // /// </summary>
    // /// <param name="dto">The DataFileSummaryDTO instance to map.</param>
    // /// <returns>A mapped DataFile instance.</returns>
    // public static DataFile ToDomain(DataFileSummaryDTO dto)
    // {
    //     if (dto.Id == null || dto.Name == null || dto.ContentType == null || dto.Size == null ||
    //         dto.CreatedStampUTC == null)
    //     {
    //         throw new ArgumentException("Id, Name, and ContentType are required fields in DataFileSummaryDTO");
    //     }
    //
    //     return new DataFile
    //     {
    //         Id = dto.Id.Value,
    //         Name = dto.Name,
    //         FileType = ContentTypeDTOMapper.ToDomain(dto.ContentType),
    //         Size = dto.Size.Value,
    //         CreatedStamp = dto.CreatedStampUTC.Value,
    //         DeletedStamp = dto.DeletedStampUTC,
    //         BeginStamp = dto.BeginStampUTC,
    //         EndStamp = dto.EndStampUTC
    //     };
    // }
}