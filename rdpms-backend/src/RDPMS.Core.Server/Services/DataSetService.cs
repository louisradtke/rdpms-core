using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataSetService(DbContext context)
    : GenericCollectionService<DataSet>(context, files => files
            .Include(ds => ds.Files)
            .ThenInclude(f => f.FileType)
            .Include(ds => ds.Files)
            .ThenInclude(f => f.Locations)
        ), IDataSetService
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

    public async Task SealDataset(Guid id)
    {
        var ds = await GetByIdAsync(id);
        if (ds == null) throw new InvalidOperationException("Dataset not found");
        if (ds.State is DataSetState.Uninitialized or DataSetState.Sealed)
        {
            ds.State = DataSetState.Sealed;
        }
        else throw new InvalidOperationException("Dataset is in invalid state");
        await UpdateAsync(ds);
    }

    public async Task<bool> ValidateSlug(string slug)
    {
        if (await Context.Set<DataSet>()
                .AnyAsync(d => d.Slug == slug))
        {
            return false;
        }
        
        return SlugUtil.IsValidSlug(slug);
    }
}
