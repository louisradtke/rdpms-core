using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IMetadataService : IGenericCollectionService<MetadataJsonField>
{
    Task<MetadataJsonField> MakeFieldFromValue(string value, ContentType contentType);
    
    /// <summary>
    /// Assigns a metadata field to an entity.
    /// Also removes any existing metadata field with the same key.
    /// If the parent collection defines a metadata column for the key/target, validation
    /// is automatically attempted using that column schema.
    /// </summary>
    /// <param name="entity">Instance of the entity</param>
    /// <param name="key">Case-insensitive key of meta date</param>
    /// <param name="value">value of meta date</param>
    Task AssignMetadate(IUniqueEntity entity, string key, MetadataJsonField value);

    /// <summary>
    /// Checks, whether the given schemaId is valid for the given metadataId and updates the meta dates validated
    /// schemas accordingly. If schema was already valid, returns true.
    /// </summary>
    /// <exception cref="InvalidOperationException">If ids fail to resolve</exception>
    /// <param name="metadateId">Id of the meta date</param>
    /// <param name="schemaId">Id of the schema</param>
    /// <param name="verbose">If true, include validator traces in result output.</param>
    /// <returns>Validation result including status and detailed reasons/traces.</returns>
    Task<ValidationResult> VerifySchema(Guid metadateId, Guid schemaId, bool verbose = false);
}
