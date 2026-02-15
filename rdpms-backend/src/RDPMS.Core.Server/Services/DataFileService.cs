using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Controllers.V1;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataFileService(
    DbContext dbContext,
    LinkGenerator linkGenerator,
    IS3Service s3Service)
    : GenericCollectionService<DataFile>(dbContext, files => files
        .Include(f => f.References)
        .Include(f => f.FileType)
        .Include(f => f.MetadataJsonFields)
        .Include(f => f.ParentDataSet)
        .ThenInclude(ds => ds!.ParentCollection)
    ), IFileService
{

    public async Task<Uri> GetFileDownloadUriAsync(Guid id, HttpContext context)
    {
        var file = await GetByIdAsync(id);
        var location = file.References
            .FirstOrDefault(lRef => lRef.StorageType == StorageType.S3);
        location ??= file.References.FirstOrDefault();

        switch (location)
        {
            case S3FileStorageReference s3Location:
                var store = dbContext.Set<DataStore>().Single(s => s.Id == s3Location.StoreFid);
                if (store is not S3DataStore s3Store)
                    throw new IllegalStateException("File ref has invalid store");
                var uriStr = await s3Service.RequestPresignedDownloadUrlAsync(s3Location, s3Store);
                return new Uri(uriStr);
            case StaticFileStorageReference staticLocation:
                return new Uri(staticLocation.URL);
            case DbFileStorageReference dbLocation:
                var uri = linkGenerator.GetUriByAction(context, nameof(FilesController.GetBlob), "Files", new { id });
                if (uri is null) throw new IllegalStateException("Uri generation has failed.");
                return new Uri(uri);
            default:
                throw new IllegalStateException("Invalid file location type");
        }
    }

    public string GetContentApiUri(Guid id, HttpContext context)
    {
        var uri = linkGenerator.GetUriByAction(
            context, nameof(FilesController.GetContent), "Files",
            new { id });
        return uri ?? throw new IllegalStateException("Invalid file location type");
    }

    public Task<FileStorageReference> GetStorageReferenceByIdAsync(Guid id)
    {
        return Context.Set<FileStorageReference>()
            .SingleAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FileStorageReference>> GetStorageReferencesAsync(
        Guid? storeId = null, StorageType? type = null)
    {
        var query = Context.Set<FileStorageReference>()
            .AsQueryable();
        if (storeId is not null)
        {
            query = query.Where(r => r.StoreFid == storeId);
        }
        if (type != null)
        {
            query = query.Where(r => r.StorageType == type);
        }
        return await query.ToListAsync();
    }

    public async Task<FileUploadTarget> RequestS3FileUploadAsync(
        DataFile file, S3FileStorageReference reference,
        Guid dataSetId, Guid dataStoreId)
    {
        var store = await Context.Set<DataStore>()
                .SingleOrDefaultAsync(s => s.Id == dataStoreId && s.StorageType == StorageType.S3)
            as S3DataStore;
        if (store is null)
        {
            throw new InvalidOperationException("Store not found or not S3!");
        }

        // var dataSetExits = await Context.Set<DataSet>()
        //     .AnyAsync(d => d.Id == dataSetId && d.State == DataSetState.Uninitialized);
        // if (!dataSetExits)
        // {
        //     throw new InvalidOperationException("DataSet not found or not in invalid state!");
        // }

        var key = $"stores/{dataStoreId}/{file.Name}";
        var uploadUrl = await s3Service.RequestPresignedUploadUrlAsync(store, key);

        reference.ObjectKey = key;
        reference.StoreFid = store.Id;

        // Context.Update(file);
        // Context.Update(reference);
        await Context.SaveChangesAsync();

        return new FileUploadTarget(new Uri(uploadUrl))
        {
            FileId = file.Id
        };
    }

    public async Task StoreInDb(DataFile file, byte[] content, StorageAttributes? referenceAttributes)
    {
        referenceAttributes ??= new StorageAttributes();

        if (referenceAttributes.SizeBytes == null)
        {
            referenceAttributes.SizeBytes = content.Length;
        }
        else if (referenceAttributes.SizeBytes != content.Length)
        {
            throw new ArgumentException("File size mismatch");
        }

        if (referenceAttributes.SHA256Hash == null)
        {
            var calculatedSha256Hash = Convert.ToHexString(SHA256.HashData(content));
            referenceAttributes.SHA256Hash = calculatedSha256Hash;
        }
        else
        {
            var calculatedSha256Hash = Convert.ToHexString(SHA256.HashData(content));
            if (referenceAttributes.SHA256Hash != calculatedSha256Hash)
            {
                throw new ArgumentException("File hash mismatch");
            }
        }

        referenceAttributes.Algorithm ??= CompressionAlgorithm.Plain;

        var storageRef = new DbFileStorageReference
        {
            Attributes = referenceAttributes,
            Data = content
        };
        file.References.Add(storageRef);

        await Context.SaveChangesAsync();
    }
    
    public async Task<IDictionary<Guid, List<string>>> GetValidatedMetadates(List<Guid> fileIds)
    {
        var result = await Context.Set<DataFile>()
            .Where(file => fileIds.Contains(file.Id))
            .Join(Context.Set<DataSet>(),
                file => file.ParentDataSetId, ds => ds.Id,
                (file, ds) => new { file, ds })
            .Join(Context.Set<DataCollectionEntity>(),
                tup => tup.ds.ParentCollectionId, collection => collection.Id,
                (tup, collection) => new { tup.file, tup.ds, collectionId = collection.Id })
            .Join(Context.Set<MetaDataCollectionColumn>(),
                tup => tup.collectionId, col => col.ParentCollectionId,
                (tup, col) => new { tup.collectionId, tup.file, tup.ds, col })
            .Join(Context.Set<DataEntityMetadataJsonField>(),
                tup => tup.file.Id, f => f.DataFileId,
                (tup, fRef)
                    => new { tup.collectionId, tup.file, tup.ds, tup.col, fRef })
            .Join(Context.Set<MetadataJsonFieldValidatedSchema>(),
                tup => tup.fRef.FieldId, rel => rel.MetadataJsonFieldId,
                (tup, rel)
                    => new { tup.collectionId, tup.file, tup.ds, tup.col, tup.fRef, schemaId = rel.JsonSchemaEntityId })
            .Where(tup => tup.col.Target == MetadataColumnTarget.File)
            .Where(tup => tup.col.MetadataKey == tup.fRef.MetadataKey)
            .Where(tup => tup.col.SchemaId == tup.schemaId)
            .GroupBy(tup => tup.file.Id)
            .ToDictionaryAsync(gr => gr.Key, gr => gr
                .Select(tup => tup.col.MetadataKey)
                .ToList());

        return result;
    }

}
