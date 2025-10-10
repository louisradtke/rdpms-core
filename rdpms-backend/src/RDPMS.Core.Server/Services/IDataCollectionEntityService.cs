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
}