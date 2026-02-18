using System.Text;
using System.Text.Json;
using Json.Schema;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class MetadataService(
    DbContext context,
    IFileService fileService,
    ISchemaService schemaService,
    ILogger<MetadataService> logger)
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
        var normalizedKey = key.ToLowerInvariant();
        var link = new DataEntityMetadataJsonField()
        {
            MetadataKey = normalizedKey,
            Field = value,
        };

        var existingRefs = Context.Set<DataEntityMetadataJsonField>()
            .Where(l => l.MetadataKey == normalizedKey)
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

        var schemaId = await ResolveCollectionColumnSchemaId(entity, normalizedKey);
        if (schemaId is null) return;

        await VerifySchema(value.Id, schemaId.Value);
    }

    public async Task<ValidationResult> VerifySchema(Guid metadateId, Guid schemaId, bool verbose = false)
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
        
        if (metadate.ValidatedSchemas.Any(s => s.Id == schemaId))
        {
            return new ValidationResult
            {
                Succesful = true,
                Reasons = []
            };
        }

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
        using var metadateDocument = JsonDocument.Parse(metadateString);
        using var schemaDocument = JsonDocument.Parse(schemaEntity.SchemaString);

        // register all local schemas in registry
        // TODO: this is ok for pre-production, but questions remain open:
        // TODO: 1. how do this on-demand? (fetching from db, what about caching?)
        // TODO: 2. What about remote schemas?
        var allSchemas = await Context.Set<JsonSchemaEntity>().ToDictionaryAsync(
            s => s.SchemaId);

        IBaseDocument? FetchFunc(Uri uri, SchemaRegistry registry)
        {
            logger.LogDebug("Fetching schema {SchemaId} from registry.", uri);
            if (!allSchemas.TryGetValue(uri.ToString(), out var schema))
            {
                logger.LogWarning("Schema {SchemaId} not found in registry.", uri);
                return null;
            }

            logger.LogDebug("Schema {SchemaId} found in registry.", uri);

            var doc = JsonDocument.Parse(schema.SchemaString);
            return new JsonElementBaseDocument(doc.RootElement, uri);
        }

        // use a local schema registry to avoid global duplicate registration collisions
        // when validating the same $id schema multiple times.
        var buildOptions = new BuildOptions
        {
            SchemaRegistry = new SchemaRegistry()
            {
                Fetch = FetchFunc
            }
        };
        var schema = JsonSchema.Build(schemaDocument.RootElement, buildOptions);
        var evaluationOptions = EvaluationOptions.From(EvaluationOptions.Default);
        if (verbose)
        {
            evaluationOptions.OutputFormat = OutputFormat.List;
        }

        var evaluationResults = schema.Evaluate(metadateDocument.RootElement, evaluationOptions);

        var result = new ValidationResult
        {
            Succesful = evaluationResults.IsValid,
            Reasons = verbose ? CollectReasons(evaluationResults) : null,
            Traces = verbose ? CollectTraces(evaluationResults) : null
        };
        
        if (!evaluationResults.IsValid) return result;
        metadate.ValidatedSchemas.Add(schemaEntity);
        Context.Update(metadate);
        await Context.SaveChangesAsync();

        return result;
    }

    private static List<string> CollectReasons(EvaluationResults root)
    {
        var reasons = new List<string>();
        Traverse(root, node =>
        {
            if (node.Errors is null) return;

            foreach (var entry in node.Errors)
            {
                reasons.Add($"[{node.InstanceLocation}] {entry.Value}");
            }
        });

        return reasons
            .Distinct()
            .ToList();
    }

    private static List<string> CollectTraces(EvaluationResults root)
    {
        var traces = new List<string>();
        Traverse(root, node =>
        {
            traces.Add(
                $"valid={node.IsValid}; evalPath={node.EvaluationPath}; instance={node.InstanceLocation}; schema={node.SchemaLocation}");
        });

        return traces;
    }

    private static void Traverse(EvaluationResults node, Action<EvaluationResults> action)
    {
        action(node);
        foreach (var detail in node.Details ?? [])
        {
            Traverse(detail, action);
        }
    }

    private async Task<Guid?> ResolveCollectionColumnSchemaId(IUniqueEntity entity, string normalizedKey)
    {
        Guid? collectionId = null;
        MetadataColumnTarget target;

        switch (entity)
        {
            case DataSet dataSet:
                collectionId = dataSet.ParentCollectionId;
                target = MetadataColumnTarget.Dataset;
                break;
            case DataFile dataFile:
                target = MetadataColumnTarget.File;

                if (dataFile.ParentDataSetId is null)
                {
                    return null;
                }

                collectionId = await Context.Set<DataSet>()
                    .Where(ds => ds.Id == dataFile.ParentDataSetId.Value)
                    .Select(ds => (Guid?)ds.ParentCollectionId)
                    .SingleOrDefaultAsync();
                break;
            default:
                throw new ArgumentException("Unknown entity type");
        }

        if (collectionId is null || collectionId == Guid.Empty)
        {
            return null;
        }

        return await Context.Set<MetaDataCollectionColumn>()
            .Where(c =>
                c.ParentCollectionId == collectionId.Value &&
                c.MetadataKey == normalizedKey &&
                c.Target == target)
            .Select(c => (Guid?)c.SchemaId)
            .SingleOrDefaultAsync();
    }
}
