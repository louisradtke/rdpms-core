using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataSetService(DbContext context)
    : GenericCollectionService<DataSet>(context), IDataSetService
{
    public async Task<IEnumerable<DataSet>> GetByCollectionAsync(Guid collectionId)
    {
        var collection = await Context.Set<DataCollectionEntity>()
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.Files)
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.AssignedTags)
            .Include(c => c.ContainedDatasets)
            .ThenInclude(d => d.AssignedLabels)
            .SingleAsync(id => id.Id == collectionId);
        
        return collection.ContainedDatasets;
    }
}
