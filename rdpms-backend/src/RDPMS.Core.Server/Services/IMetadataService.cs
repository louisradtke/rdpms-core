using System.Text;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public interface IMetadataService
{
    public Task<MetadataJsonField> MakeFieldFromValue(string value, ContentType contentType);
    
    /// <summary>
    /// Assigns a metadata field to an entity.
    /// Also removes any existing metadata field with the same key.
    /// </summary>
    /// <param name="entity">Instance of the entity</param>
    /// <param name="key">Case-insensitive key of meta date</param>
    /// <param name="value">value of meta date</param>
    public Task AssignMetadate(IUniqueEntity entity, string key, MetadataJsonField value);
}

public class MetadataService(DbContext context, IFileService fileService) : IMetadataService
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
            Value = file
        };

        context.Add(field);
        await context.SaveChangesAsync();
        
        return field;
    }

    public async Task AssignMetadate(IUniqueEntity entity, string key, MetadataJsonField value)
    {
        var link = new DataEntityMetadataJsonField()
        {
            MetadataKey = key,
            MetadataJsonField = value,
        };

        var existingRefs = context.Set<DataEntityMetadataJsonField>()
            .Where(l => l.MetadataKey == key)
            .AsQueryable();
        switch (entity)
        {
            case DataSet dataSet:
                existingRefs = existingRefs.Where(l => l.DataSetId == dataSet.Id);
                break;
            case DataFile file:
                existingRefs = existingRefs.Where(l => l.DataFileId == file.Id);
                break;
            default:
                throw new ArgumentException("Unknown entity type");
        }
        
        context.RemoveRange(existingRefs.Cast<object>().AsEnumerable());

        context.Add(link);
        await context.SaveChangesAsync();
    }
}