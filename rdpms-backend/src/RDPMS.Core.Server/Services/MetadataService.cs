using System.Text;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class MetadataService(DbContext context, IFileService fileService)
    : GenericCollectionService<MetadataJsonField>(context, q => q
        .Include(f => f.Value)
        .ThenInclude(f => f!.FileType)
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
}