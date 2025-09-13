using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public interface IDataSetRepository : IGenericRepository<DataSet>
{
    /// <summary>
    /// Retrieves a collection of datasets associated with the specified collection ID.
    /// </summary>
    /// <param name="collectionId">The unique identifier of the collection whose datasets are to be retrieved.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the collection ID does not exist or if there is an issue
    /// during the retrieval process.</exception>
    Task<IEnumerable<DataSet>> GetByCollectionIdAsync(Guid collectionId);
}