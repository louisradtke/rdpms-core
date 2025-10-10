using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Services;

public class SlugResolvingService<T>(DbContext context) where T : class, IUniqueEntityWithSlug
{
    public DbContext Context { get; } = context;

    /// <summary>
    /// Resolves a slug and returns the item.
    /// </summary>
    /// <param name="slug">The slug to resolve and query from the db.</param>
    /// <returns>Instance of the item, if found. (Exception otherwise)</returns>
    /// <exception cref="IllegalArgumentException">If slug is invalid, according to
    /// <see cref="SlugUtil.IsValidSlug"/></exception>
    /// <exception cref="InvalidOperationException">If no match is found, or match is not unique.</exception>
    public async Task<T> GetBySlugOrId(string slug)
    {
        if (!SlugUtil.IsValidSlug(slug))
        {
            throw new IllegalArgumentException("Slug is invalid.");
        }
        
        return await Context.Set<T>()
            .SingleAsync(t => t.Slug == slug);
    }
}