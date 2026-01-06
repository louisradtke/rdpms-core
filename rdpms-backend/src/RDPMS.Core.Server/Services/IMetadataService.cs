using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IMetadataService : IGenericCollectionService<MetadataJsonField>
{
    public Task<MetadataJsonField> MakeFieldFromValue(string value, ContentType contentType);
    
    /// <summary>
    /// Assigns a metadata field to an entity.
    /// Also removes any existing metadata field with the same key.
    /// </summary>
    /// <param name="entity">Instance of the entity</param>
    /// <param name="key">Case-insensitive key of meta date</param>
    /// <param name="value">value of meta date</param>
    public Task AssignMetadate(IUniqueEntity entity, string key, MetadataJsonField value);
}