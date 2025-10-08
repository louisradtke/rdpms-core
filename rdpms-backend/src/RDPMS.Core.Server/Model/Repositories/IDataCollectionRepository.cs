using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public interface IDataCollectionRepository : IGenericRepository<DataCollectionEntity>
{
    Task<IEnumerable<DataCollectionEntity>> GetAllInProject(Guid projectId);


    /// <summary>
    /// Get the number of datasets in a collection.
    /// </summary>
    /// <param name="collectionIds">Enumerable of ids</param>
    /// <returns>The count for each id.</returns>
    Task<IEnumerable<int>> GetDatasetCounts(IEnumerable<Guid> collectionIds);
}