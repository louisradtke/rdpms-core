using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class ContentTypeDTOMapper
{
    /// <summary>
    /// Maps a <see cref="ContentType"/> to a <see cref="ContentTypeDTO"/>.
    /// </summary>
    /// <param name="domain">The ContentType instance to map.</param>
    /// <returns>A mapped ContentTypeDTO instance.</returns>
    public static ContentTypeDTO ToDTO(ContentTypeEntity domain)
    {
        if (domain == null)
        {
            throw new ArgumentNullException(nameof(domain));
        }

        return new ContentTypeDTO
        {
            Id = domain.Id == Guid.Empty ? null : domain.Id,
            Abbreviation = domain.Abbreviation,
            DisplayName = domain.Name ?? domain.Abbreviation.ToUpper(),
            Description = domain.Description,
            MimeType = domain.MimeType
        };
    }

    /// <summary>
    /// Maps a <see cref="ContentTypeDTO"/> to a <see cref="ContentType"/>.
    /// </summary>
    /// <param name="dto">The ContentTypeDTO instance to map.</param>
    /// <returns>A mapped ContentType instance.</returns>
    public static ContentTypeEntity ToDomain(ContentTypeDTO dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto));
        }

        return new ContentTypeEntity()
        {
            Id = dto.Id ?? Guid.NewGuid(),
            Abbreviation = dto.Abbreviation ?? string.Empty,
            Name = dto.DisplayName,
            Description = dto.Description ?? string.Empty,
            MimeType = dto.MimeType
        };
    }
}