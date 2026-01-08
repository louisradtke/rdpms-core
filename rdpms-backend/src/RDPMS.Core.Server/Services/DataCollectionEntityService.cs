using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataCollectionEntityService(DbContext dbContext)
    : GenericCollectionService<DataCollectionEntity>(dbContext, q => q
        .Include(c => c.ContainedDatasets)
        .Include(c => c.DefaultDataStore)
        .Include(c => c.MetaDataColumns)
        .ThenInclude(c => c.DefaultField)
        .Include(c => c.MetaDataColumns)
        .ThenInclude(c => c.Schema)
    ), IDataCollectionEntityService
{
    public Task<Dictionary<Guid, int>> GetDatasetCounts(IEnumerable<Guid> collectionIds)
    {
        var collectionIdsList = collectionIds.ToList();
    
        return Context.Set<DataCollectionEntity>()
            .Where(c => collectionIdsList.Contains(c.Id))
            .Select(c => new { c.Id, Count = c.ContainedDatasets.Count })
            .ToDictionaryAsync(x => x.Id, x => x.Count);
    }

    public async Task<bool> UpsertMetaDataColumnAsync(Guid collectionId, string key, Guid schemaId,
        Guid? defaultMetadataId)
    {
        var normalizedKey = key.ToLowerInvariant();
        var updated = await Context.Set<MetaDataCollectionColumn>()
            .Where(c => c.ParentCollectionId == collectionId && c.MetadataKey == normalizedKey)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.SchemaId, schemaId)
                .SetProperty(c => c.DefaultFieldId, defaultMetadataId));

        if (updated > 0)
        {
            return false;
        }

        var column = new MetaDataCollectionColumn
        {
            ParentCollectionId = collectionId,
            MetadataKey = normalizedKey,
            SchemaId = schemaId,
            DefaultFieldId = defaultMetadataId
        };

        Context.Set<MetaDataCollectionColumn>().Add(column);
        await Context.SaveChangesAsync();
        return true;
    }
}
