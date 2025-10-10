using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories.Infra;

namespace RDPMS.Core.Server.Model.Repositories;

public interface ISlugRepository : IGenericRepository<Slug>
{
    /// <summary>
    /// Check, if this project slug is already taken.
    /// </summary>
    /// <param name="slug">The requested slug.</param>
    /// <returns>True, if the slug is already taken. False otherwise.</returns>
    Task<bool> IsProjectSlugTakenAsync(string slug);

    /// <summary>
    /// Checks, if a slug is taken in the scope of a given parent.
    /// </summary>
    /// <param name="child">Entity with parent. Only used for type inference.</param>
    /// <param name="slug">The requested slug.</param>
    /// <param name="parentId">Guid of parent.</param>
    /// <typeparam name="TChild">Type of the entity to check for.</typeparam>
    /// <returns>True, if the slug is already taken. False otherwise.</returns>
    Task<bool> IsSlugTakenGivenParent<TChild>(TChild child, string slug, Guid parentId)
        where TChild : class, IUniqueEntityWithParent;

    /// <summary>
    /// Checks, if a slug is taken in the scope of a given parent.
    /// </summary>
    /// <param name="slug">The requested slug.</param>
    /// <param name="parentId">Guid of parent.</param>
    /// <typeparam name="TChild">Type of the entity to check .</typeparam>
    /// <returns>True, if the slug is already taken. False otherwise.</returns>
    Task<bool> IsSlugTakenGivenParent<TChild>(string slug, Guid parentId)
        where TChild : class, IUniqueEntityWithParent;

    /// <summary>
    /// Get all slugs entities that match the given slug and type.
    /// </summary>
    /// <param name="slug">Slug string.</param>
    /// <param name="type">The slug type to filter for. Null means, no filtering will be applied.</param>
    /// <returns>All matching slugs.</returns>
    Task<IEnumerable<Slug>> GetMatchingSlugs(string slug, SlugType? type);
    
    /// <summary>
    /// Get all the slugs for a given entity.
    /// </summary>
    /// <param name="entityId">Id of the entity.</param>
    /// <returns>List of all slugs representing the entity.</returns>
    Task<IEnumerable<Slug>> GetSlugsForEntity(Guid entityId);

    /// <summary>
    /// Get all the slugs for the set of entities.
    /// </summary>
    /// <param name="entityIds">Enum of all slugs</param>
    /// <param name="slugType">Type of the slugs.</param>
    /// <returns>Unordered enumerable of slugs.</returns>
    Task<IEnumerable<Slug>> GetSlugsForEntities(IEnumerable<Guid> entityIds, SlugType slugType);
    
    /// <summary>
    /// Set all slugs but the given one to deprecated.
    /// </summary>
    /// <param name="slug">Slug to keep.</param>
    /// <param name="entityId">Id of the entity to scope.</param>
    Task SetDeprecatedButAsync(string slug, Guid entityId);
}