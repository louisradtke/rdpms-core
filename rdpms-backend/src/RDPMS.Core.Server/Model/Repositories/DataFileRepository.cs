using CommandLine;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataFileRepository(RDPMSPersistenceContext ctx)
{
    public async Task<IEnumerable<DataFile>> GetFilesAsync()
    {
        // todo: test cast
        return await ctx.DataFiles.ToListAsync();
    }

    public async Task<DataFile> GetFileAsync(Guid id)
    {
        var entity = await ctx.DataFiles.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException();
        return entity;
    }

    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        var storeEntity = await ctx.DataStores.FindAsync(storeId);
        if (storeEntity == null) return new List<DataFile>();

        await ctx.Entry(storeEntity).Reference(s => s.DataFiles).LoadAsync();
        return storeEntity.DataFiles;
    }

    public async Task<FileUploadTarget> AddFileAsync(DataFile entity)
    {
        ctx.DataFiles.Add(entity);
        await ctx.SaveChangesAsync();
        // TODO: gen URL
        return new FileUploadTarget(new Uri("https://todo.com"));
    }
}