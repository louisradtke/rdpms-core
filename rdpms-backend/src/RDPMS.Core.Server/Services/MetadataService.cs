using System.Text;
using System.Text.Json;
using Json.Schema;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class MetadataService(
    DbContext context,
    IFileService fileService,
    ISchemaService schemaService)
    : GenericCollectionService<MetadataJsonField>(context, q => q
        .Include(f => f.Value)
        .ThenInclude(f => f!.FileType)
        .Include(f => f.Value)
        .ThenInclude(f => f!.References)
        .Include(f => f.ValidatedSchemas)
    ), IMetadataService
{
    public async Task<MetadataJsonField> MakeFieldFromValue(string value, ContentType contentType)
    {
        var id = Guid.NewGuid();
        var file = new DataFile(id.ToString())
        {
            Id = id,
            FileType = contentType,
        };

        var content = Encoding.UTF8.GetBytes(value);
        await fileService.StoreInDb(file, content, null);

        var field = new MetadataJsonField()
        {
            Value = file,
        };

        Context.Add(file);
        Context.Add(field);
        await Context.SaveChangesAsync();
        
        return field;
    }

    public async Task AssignMetadate(IUniqueEntity entity, string key, MetadataJsonField value)
    {
        var link = new DataEntityMetadataJsonField()
        {
            MetadataKey = key,
            MetadataJsonField = value,
        };

        var existingRefs = Context.Set<DataEntityMetadataJsonField>()
            .Where(l => l.MetadataKey == key)
            .AsQueryable();
        switch (entity)
        {
            case DataSet dataSet:
                existingRefs = existingRefs.Where(l => l.DataSetId == dataSet.Id);
                link.DataSetId = dataSet.Id;
                break;
            case DataFile dataFile:
                existingRefs = existingRefs.Where(l => l.DataFileId == dataFile.Id);
                link.DataFileId = dataFile.Id;
                break;
            default:
                throw new ArgumentException("Unknown entity type");
        }
        
        Context.RemoveRange(existingRefs.Cast<object>().AsEnumerable());

        Context.Add(link);
        await Context.SaveChangesAsync();
    }

    public async Task<bool> VerifySchema(Guid metadateId, Guid schemaId)
    {
        var metadate = await GetByIdAsync(metadateId);
        DbFileStorageReference? metadateRef;
        try
        {
            metadateRef = metadate
                    .Value?
                    .References
                    .Single(r => r.StorageType == StorageType.Db)
                as DbFileStorageReference;
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException("Single() LINQ query failed for metadateId", e);
        }
        if (metadateRef is null) throw new InvalidOperationException("Metadate reference is null");
        
        if (metadate.ValidatedSchemas.Any(s => s.Id == schemaId)) return true;

        JsonSchemaEntity schemaEntity = null!;
        try
        {
            schemaEntity = await schemaService.Query()
                .SingleAsync(s => s.Id == schemaId);
        }
        catch (InvalidOperationException e)
        {
            throw new InvalidOperationException("Single() LINQ query failed for schemaId", e);
        }
        
        var metadateString = Encoding.UTF8.GetString(metadateRef.Data);
        var metadateJson = JsonDocument.Parse(metadateString).RootElement;
        var schemaJson = JsonDocument.Parse(schemaEntity.SchemaString).RootElement;
        var schema = JsonSchema.Build(schemaJson); // optionally include build options
        var evaluationResults = schema.Evaluate(metadateJson);

        if (!evaluationResults.IsValid) return false;
        metadate.ValidatedSchemas.Add(schemaEntity);
        Context.Update(metadate);
        await Context.SaveChangesAsync();

        return true;
    }
}