using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services;

public interface ISlugService
{
    Task<string> RegisterSlugAsync<TEntity>(TEntity entity, string requestedSlug)
        where TEntity : class, IUniqueEntity;

    /// <summary>
    /// Resolve a project by slug.
    /// </summary>
    /// <param name="slug">The slug name.</param>
    /// <returns>The project instance, if the project could be resolved. null otherwise.</returns>
    Task<Project?> ResolveProjectBySlugAsync(string slug);
    
    /// <summary>
    /// Resolve an entity by slug, given its parent's id.
    /// </summary>
    /// <param name="slug">The slug name.</param>
    /// <param name="parentId"></param>
    /// <returns>The entities instance, if it could be resolved. null Otherwise.</returns>
    Task<TEntity?> ResolveEntityWithParentBySlugAsync<TEntity>(string slug, Guid parentId)
        where TEntity : class, IUniqueEntityWithNullableParent;
    
    /// <summary>
    /// Get all slugs for a given entity.
    /// </summary>
    /// <param name="entityId">The entity to find the slugs for.</param>
    /// <typeparam name="TEntity"></typeparam>
    /// <returns></returns>
    Task<IEnumerable<Slug>> GetSlugsForEntityAsync<TEntity>(Guid entityId)
        where TEntity : class, IUniqueEntity;
}