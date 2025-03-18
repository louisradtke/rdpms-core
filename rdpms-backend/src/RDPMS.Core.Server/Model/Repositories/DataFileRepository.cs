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
        return await Task.Run(() => ctx.DataFiles.Select(DataFileEntityMapper.ToDomain).ToList());
        // return await _ctx.DataFiles.Select(DataFileEntityMapper.ToDomain).ToListAsync();
    }

    public async Task<DataFile> GetFileAsync(Guid id)
    {
        var entity = await ctx.DataFiles.FindAsync(id);
        if (entity == null) throw new KeyNotFoundException();
        return DataFileEntityMapper.ToDomain(entity);
    }

    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        var storeEntity = await ctx.DataStores.FindAsync(storeId);
        if (storeEntity == null) return new List<DataFile>();

        await ctx.Entry(storeEntity).Reference(s => s.DataFiles).LoadAsync();
        return storeEntity.DataFiles.Select(DataFileEntityMapper.ToDomain);
    }

    public async Task<FileUploadTarget> AddFileAsync(DataFile file)
    {
        var entity = DataFileEntityMapper.ToEntity(file);
        ctx.DataFiles.Add(entity);
        await ctx.SaveChangesAsync();
        // TODO: gen URL
        return new FileUploadTarget(new Uri("https://todo.com"));
    }
}