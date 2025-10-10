using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public class SlugRepository(RDPMSPersistenceContext ctx) : GenericRepository<Slug>(ctx), ISlugRepository
{
    private readonly DbSet<Project> _projects = ctx.Set<Project>();
    private readonly DbSet<Slug> _slugs = ctx.Set<Slug>();

    public async Task<bool> IsProjectSlugTakenAsync(string slug)
    {
        if (!await _slugs.AnyAsync(s => s.Value == slug))
        {
            return false;
        }

        var query = from s in _slugs
            where s.Value == slug
            join p in _projects on s.EntityId equals p.Id
            select s;
        return await query.AnyAsync();
    }

    public async Task<bool> IsSlugTakenGivenParent<TChild>(TChild child, string slug, Guid parentId)
        where TChild : class, IUniqueEntityWithParent
    {
        return await IsSlugTakenGivenParent<TChild>(slug, parentId);
    }

    public async Task<bool> IsSlugTakenGivenParent<TChild>(string slug, Guid parentId)
        where TChild : class, IUniqueEntityWithParent
    {
        if (!await _slugs.AnyAsync(s => s.Value == slug))
        {
            return false;
        }
        
        var tDbSet = Context.Set<TChild>();

        var query = from s in _slugs
            where s.Value == slug
            join t in tDbSet on s.EntityId equals t.Id
            where t.ParentId == parentId
            select s;

        return await query
            .AsNoTracking()
            .AnyAsync();
    }

    public async Task<IEnumerable<Slug>> GetMatchingSlugs(string slug, SlugType? type)
    {
        var query = _slugs.AsQueryable();
        if (type != null)
        {
            query = query.Where(s => s.Type == type);
        }
        return await query
            .AsNoTracking()
            .Where(s => s.Value == slug)
            .ToListAsync();
    }

    public async Task<IEnumerable<Slug>> GetSlugsForEntity(Guid entityId)
    {
        return await _slugs
            .AsNoTracking()
            .Where(s => s.EntityId == entityId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Slug>> GetSlugsForEntities(IEnumerable<Guid> entityIds, SlugType slugType)
    {
        var entityIdsList = entityIds.ToList();
        return await DbSet
            .AsNoTracking()
            .Where(s => entityIdsList.Contains(s.EntityId) && s.Type == slugType)
            .ToListAsync();
    }

    public async Task SetDeprecatedButAsync(string slug, Guid entityId)
    {
        await _slugs
            .Where(s => s.Value != slug &&
                        s.EntityId == entityId &&
                        s.State != SlugState.Deprecated)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.State, SlugState.Deprecated));
    }
}