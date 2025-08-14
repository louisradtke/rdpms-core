using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class ProjectRepository(RDPMSPersistenceContext ctx) : GenericRepository<Project>(ctx), IProjectRepository;