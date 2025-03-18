using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;

namespace RDPMS.Core.Server.Model.Mappers;

public static class DataFileEntityMapper
{
    /// <summary>
    /// Maps a <see cref="DataFileEntity"/> to a <see cref="DataFile"/>.
    /// </summary>
    /// <param name="entity">The DataFileEntity instance to map.</param>
    /// <returns>A mapped DataFile instance.</returns>
    public static DataFile ToDomain(DataFileEntity entity)
    {
        return new DataFile
        {
            Id = entity.Id,
            Name = entity.Name,
            FileType = ContentTypeEntityMapper.ToDomain(entity.FileType),
            Size = entity.Size,
            Hash = entity.Hash,
            CreatedStamp = entity.CreatedStamp,
            DeletedStamp = entity.DeletedStamp,
            BeginStamp = entity.BeginStamp,
            EndStamp = entity.EndStamp
        };
    }

    /// <summary>
    /// Maps a <see cref="DataFile"/> to a <see cref="DataFileEntity"/>.
    /// </summary>
    /// <param name="domain">The DataFile instance to map.</param>
    /// <returns>A mapped DataFileEntity instance.</returns>
    public static DataFileEntity ToEntity(DataFile domain)
    {
        return new DataFileEntity(domain.Name)
        {
            Id = domain.Id,
            FileType = ContentTypeEntityMapper.ToEntity(domain.FileType),
            Size = domain.Size,
            Hash = domain.Hash,
            CreatedStamp = domain.CreatedStamp,
            DeletedStamp = domain.DeletedStamp,
            BeginStamp = domain.BeginStamp,
            EndStamp = domain.EndStamp
        };
    }
}