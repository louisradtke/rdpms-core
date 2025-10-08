using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IDataSetService : IGenericCollectionService<DataSet>
{
    Task<IEnumerable<DataSet>> GetByCollectionAsync(Guid collectionId);
    Task UpdateFieldsAsync(Guid id, DataSet updates);
}