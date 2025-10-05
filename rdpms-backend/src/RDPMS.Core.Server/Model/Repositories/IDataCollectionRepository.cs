using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public interface IDataCollectionRepository : IGenericRepository<DataCollectionEntity>
{
    Task<IEnumerable<DataCollectionEntity>> GetAllInProject(Guid projectId);
}