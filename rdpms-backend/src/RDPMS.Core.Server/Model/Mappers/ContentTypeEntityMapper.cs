using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class ContentTypeEntityMapper
{
    /// <summary>
    /// Maps a <see cref="ContentTypeEntity"/> to a <see cref="ContentType"/>.
    /// </summary>
    /// <param name="entity">The ContentTypeEntity instance to map.</param>
    /// <returns>A mapped ContentType instance.</returns>
    public static ContentType ToDomain(ContentTypeEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new ContentType
        {
            Id = entity.Id,
            Abbreviation = entity.Abbreviation,
            Name = entity.Name,
            Description = entity.Description,
            MimeType = entity.MimeType
        };
    }

    /// <summary>
    /// Maps a <see cref="ContentType"/> to a <see cref="ContentTypeEntity"/>.
    /// </summary>
    /// <param name="domain">The ContentType instance to map.</param>
    /// <returns>A mapped ContentTypeEntity instance.</returns>
    public static ContentTypeEntity ToEntity(ContentType domain)
    {
        return new ContentTypeEntity
        {
            Id = domain.Id,
            Abbreviation = domain.Abbreviation,
            Name = domain.Name,
            Description = domain.Description,
            MimeType = domain.MimeType
        };
    }
}