using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class ContainerService : GenericCollectionService<DataContainer>, IContainerService
{
    public ContainerService(DataContainerRepository repo) : base(repo)
    {
    }
}