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

    public async Task<bool> UpsertMetaDataColumnAsync(
        Guid collectionId,
        string key,
        Guid schemaId,
        Guid? defaultMetadataId,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset)
    {
        var normalizedKey = key.ToLowerInvariant();
        var updated = await Context.Set<MetaDataCollectionColumn>()
            .Where(c =>
                c.ParentCollectionId == collectionId &&
                c.MetadataKey == normalizedKey &&
                c.Target == target)
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
            DefaultFieldId = defaultMetadataId,
            Target = target
        };

        Context.Set<MetaDataCollectionColumn>().Add(column);
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task RenameColumnAsync(
        Guid collectionId,
        string oldKey,
        string newKey,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset)
    {
        var normalizedOldKey = oldKey.ToLowerInvariant();
        var normalizedNewKey = newKey.ToLowerInvariant();
        var updated = await Context.Set<MetaDataCollectionColumn>()
            .Where(c =>
                c.ParentCollectionId == collectionId &&
                c.MetadataKey == normalizedOldKey &&
                c.Target == target)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.MetadataKey, normalizedNewKey));
        if (updated == 0)
        {
            throw new InvalidOperationException("No such column.");
        }
    }

    public async Task DeleteColumnAsync(
        Guid collectionId,
        string key,
        MetadataColumnTarget target = MetadataColumnTarget.Dataset)
    {
        var normalizedKey = key.ToLowerInvariant();
        var deleted = await Context.Set<MetaDataCollectionColumn>()
            .Where(c =>
                c.ParentCollectionId == collectionId &&
                c.MetadataKey == normalizedKey &&
                c.Target == target)
            .ExecuteDeleteAsync();
        
        if (deleted == 0)
        {
            throw new InvalidOperationException("No such column.");
        }
    }
}
