using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IDataCollectionEntityService : IGenericCollectionService<DataCollectionEntity>
{
    /// <summary>
    /// Get the number of datasets in a collection.
    /// </summary>
    /// <param name="collectionIds">Enumerable of ids</param>
    /// <returns>A dictionary mapping counts to Ids.</returns>
    Task<Dictionary<Guid, int>> GetDatasetCounts(IEnumerable<Guid> collectionIds);

    /// <summary>
    /// Add or update a metadata column for a collection.
    /// </summary>
    /// <param name="collectionId">Id of the collection</param>
    /// <param name="key">Key of the column</param>
    /// <param name="schemaId">Id of the schema</param>
    /// <param name="defaultMetadataId">Optional id of the default metadata</param>
    /// <param name="target">Target entity type this column applies to.</param>
    /// <returns>True when a new column was created, false when an existing column was updated.</returns>
    Task<bool> UpsertMetaDataColumnAsync(
        Guid collectionId,
        string key,
        Guid schemaId,
        Guid? defaultMetadataId,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset);

    /// <summary>
    /// Rename a metadata column.
    /// </summary>
    /// <param name="collectionId">Id of the collection</param>
    /// <param name="oldKey">The old key</param>
    /// <param name="newKey">The new key</param>
    /// <exception cref="InvalidOperationException">If the column does not exist.</exception>
    /// <returns></returns>
    Task RenameColumnAsync(
        Guid collectionId,
        string oldKey,
        string newKey,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset);

    Task DeleteColumnAsync(
        Guid collectionId,
        string key,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset);
}
