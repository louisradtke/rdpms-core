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
}