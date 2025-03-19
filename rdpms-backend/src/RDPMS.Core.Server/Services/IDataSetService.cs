using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public interface IDataSetService : IGenericCollectionService<DataSet>;

public class DataSetService(
    IGenericRepository<DataSet> repo) : GenericCollectionService<DataSet>(repo),
    IDataSetService;