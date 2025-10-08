using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IDataCollectionEntityService : IGenericCollectionService<DataCollectionEntity>
{
    /// <summary>
    /// Get the number of datasets in a collection.
    /// </summary>
    /// <param name="collectionIds">Enumerable of ids</param>
    /// <returns>The count for each id.</returns>
    Task<IEnumerable<int>> GetDatasetCounts(IEnumerable<Guid> collectionIds);
}