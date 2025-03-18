using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;

namespace RDPMS.Core.Server.Model.Repositories;

public class ContentTypeRepository(RDPMSPersistenceContext ctx)
{
    public async Task<IEnumerable<ContentTypeEntity>> GetAllAsync()
    {
        var entities = await ctx.Types.ToListAsync();
        return entities;
    }

    public async Task<ContentTypeEntity> GetByIdAsync(Guid id)
    {
        var entity = await ctx.Types.FindAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException();
        }

        return entity;
    }
    
    public async Task<bool> CheckForIdAsync(Guid id)
    {
        // Console.WriteLine(await ctx.Types.CountAsync());
        var any = (await ctx.Types.ToListAsync()).Any(t => t.Id == id);
        return any;
    }
    
    public async Task AddAsync(ContentTypeEntity entity)
    {
        await ctx.Types.AddAsync(entity);
        await ctx.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<ContentTypeEntity> entities)
    {
        await ctx.Types.AddRangeAsync(entities);
        await ctx.SaveChangesAsync();
    }
}