using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public class ContentTypeDTOMapper : IImportMapper<ContentType, ContentTypeDTO>,
    IExportMapper<ContentType, ContentTypeDTO>
{
    /// <summary>
    /// Maps a <see cref="ContentType"/> to a <see cref="ContentTypeDTO"/>.
    /// </summary>
    /// <param name="domain">The ContentType instance to map.</param>
    /// <returns>A mapped ContentTypeDTO instance.</returns>
    public ContentTypeDTO Export(ContentType domain)
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
    /// <param name="foreign">The ContentTypeDTO instance to map.</param>
    /// <returns>A mapped ContentType instance.</returns>
    public ContentType Import(ContentTypeDTO foreign)
    {
        if (foreign == null)
        {
            throw new ArgumentNullException(nameof(foreign));
        }

        return new ContentType()
        {
            Id = foreign.Id ?? Guid.NewGuid(),
            Abbreviation = foreign.Abbreviation ?? string.Empty,
            Name = foreign.DisplayName,
            Description = foreign.Description ?? string.Empty,
            MimeType = foreign.MimeType
        };
    }

    public IEnumerable<CheckSet<ContentTypeDTO>> ImportChecks() => [];
}