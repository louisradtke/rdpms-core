using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public interface IDataFileRepository : IGenericRepository<DataFile>
{
    Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId);
}