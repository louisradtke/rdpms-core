using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataSetService(DbContext context, IS3Service s3Service)
    : GenericCollectionService<DataSet>(context, files => files
        .Include(ds => ds.ParentCollection)
        .Include(ds => ds.MetadataJsonFields)
        .ThenInclude(f => f.Field)
        .ThenInclude(f => f.Value)
        .ThenInclude(f => f!.FileType)
        .Include(ds => ds.MetadataJsonFields)
        .ThenInclude(f => f.Field)
        .ThenInclude(f => f.ValidatedSchemas)
        .Include(ds => ds.MetadataJsonFields)
        .ThenInclude(f => f.Field)
        .ThenInclude(f => f.Value)
        .ThenInclude(f => f!.References)
        .Include(ds => ds.Files)
        .ThenInclude(f => f.FileType)
        .Include(ds => ds.Files)
        .ThenInclude(f => f.References)
        .Include(ds => ds.Files)
        .ThenInclude(f => f.MetadataJsonFields)
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

        var locations = ds.Files
            .SelectMany(f => f.References.Where(l => l.StorageType == StorageType.S3))
            .ToList();

        var storeIds = locations
            .Where(l => l.StoreFid is not null || l.StoreFid != Guid.Empty)
            .Select(l => l.StoreFid)
            .Distinct()
            .ToList();
        var storeDict = Context.Set<DataStore>()
            .Where(s => storeIds.Contains(s.Id))
            .ToDictionary(s => s.Id);
        var checkTasks = locations
            .Select(l => Task.Run(() =>
                    s3Service.ValidateFileRef((S3FileStorageReference)l, (S3DataStore) storeDict[l.StoreFid!.Value])));

        var success = true;
        foreach (var task in checkTasks)
        {
            try
            {
                success = success && await task;
            }
            catch (Exception)
            {
                success = false;
            }
        }

        if (!success)
        {
            throw new InvalidOperationException("Could not validate all files.");
        }

        if (ds.LifecycleState is DataSetState.Uninitialized or DataSetState.Sealed)
        {
            ds.LifecycleState = DataSetState.Sealed;
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

    public async Task<IDictionary<Guid, List<string>>> GetValidatedMetadates(List<Guid> datasetIds)
    {
        var result = await Context.Set<DataSet>()
            .Where(ds => datasetIds.Contains(ds.Id))
            .Join(Context.Set<DataCollectionEntity>(),
                ds => ds.ParentCollectionId, collection => collection.Id,
                (ds, collection) => new { ds, collectionId = collection.Id })
            .Join(Context.Set<MetaDataCollectionColumn>(),
                tup => tup.collectionId, col => col.ParentCollectionId,
                (tup, col) => new { collectionId = tup.collectionId, tup.ds, col })
            .Join(Context.Set<DataEntityMetadataJsonField>(),
                tup => tup.ds.Id, f => f.DataSetId,
                (tup, fRef)
                    => new { tup.collectionId, tup.ds, tup.col, fRef })
            .Join(Context.Set<MetadataJsonFieldValidatedSchema>(),
                tup => tup.fRef.FieldId, rel => rel.MetadataJsonFieldId,
                (tup, rel)
                    => new { tup.collectionId, tup.ds, tup.col, tup.fRef, schemaId = rel.JsonSchemaEntityId })
            .Where(tup => tup.col.Target == MetadataColumnTarget.Dataset)
            .Where(tup => tup.col.MetadataKey == tup.fRef.MetadataKey)
            .Where(tup => tup.col.SchemaId == tup.schemaId)
            .GroupBy(tup => tup.ds.Id)
            .ToDictionaryAsync(gr => gr.Key, gr => gr
                .Select(tup => tup.col.MetadataKey)
                .ToList());

        return result;
    }
}
