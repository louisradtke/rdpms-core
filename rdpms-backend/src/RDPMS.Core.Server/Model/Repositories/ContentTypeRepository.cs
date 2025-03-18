using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;

namespace RDPMS.Core.Server.Model.Repositories;

public class ContentTypeRepository(RDPMSPersistenceContext ctx)
{
    public async Task<IEnumerable<ContentType>> GetAllAsync()
    {
        var entities = await ctx.Types.ToListAsync();
        return entities.Select(ContentTypeEntityMapper.ToDomain).ToList();
    }

    public async Task<ContentType> GetByIdAsync(Guid id)
    {
        var entity = await ctx.Types.FindAsync(id);
        if (entity == null)
        {
            throw new KeyNotFoundException();
        }

        return ContentTypeEntityMapper.ToDomain(entity);
    }
    
    public async Task<bool> CheckForIdAsync(Guid id)
    {
        // Console.WriteLine(await ctx.Types.CountAsync());
        var any = (await ctx.Types.ToListAsync()).Any(t => t.Id == id);
        return any;
    }
    
    public async Task AddAsync(ContentType contentType)
    {
        var entity = ContentTypeEntityMapper.ToEntity(contentType);
        ctx.Types.Add(entity);
        await ctx.SaveChangesAsync();
    }
}