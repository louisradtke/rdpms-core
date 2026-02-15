using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IDataSetService : IGenericCollectionService<DataSet>
{
    Task<IEnumerable<DataSet>> GetByCollectionAsync(Guid collectionId);

    /// <summary>
    /// Set the state of a data set to sealed (idempotent).
    /// </summary>
    /// <param name="id">The id of the dataset.</param>
    /// <exception cref="InvalidOperationException">Thrown, if data set is not found or in invalid state.</exception>
    public Task SealDataset(Guid id);

    /// <summary>
    /// Check, whether slug is unique and fulfils the slug constraints.
    /// </summary>
    /// <param name="slug">slug to check</param>
    /// <returns>true if valid, false otherwise</returns>
    Task<bool> ValidateSlug(string slug);

    /// <summary>
    /// Get a list of keys, where metadata matches the schemas declared for the parent collection.
    /// </summary>
    /// <param name="datasetIds">The ids of the datasets to check</param>
    /// <returns>A dictionary mapping the dataset id to the list of keys, of which the values were validated.</returns>
    /// <exception cref="InvalidOperationException">Thrown, if datasets don't share same parent collection.</exception>
    Task<IDictionary<Guid, List<string>>> GetValidatedMetadates(List<Guid> datasetIds);
}