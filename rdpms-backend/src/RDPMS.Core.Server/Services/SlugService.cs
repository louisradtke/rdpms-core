using System.Text.RegularExpressions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;

namespace RDPMS.Core.Server.Services;

public class SlugService(
    ISlugRepository slugRepository,
    IProjectRepository projectRepository,
    IDataCollectionRepository collectionRepository,
    IDataSetRepository dataSetRepository,
    IDataStoreRepository dataStoreRepository
) : ISlugService
{
    private static readonly Random Random = new();
    private static readonly char[] AlphanumericChars = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
    public static readonly Regex SlugRegex = new(@"^[a-zA-Z0-9\+\.\-_]+$", RegexOptions.Compiled);
    public static readonly int InitSuffixLength = 4;

    public async Task<string> RegisterSlugAsync<TEntity>(TEntity entity, string requestedSlug)
        where TEntity : class, IUniqueEntity
    {
        if (!SlugUtil.IsQualifiedForSlug(typeof(TEntity)))
        {
            throw new ArgumentException("Type is not qualified for slugs");
        }

        if (!SlugRegex.IsMatch(requestedSlug))
        {
            throw new ArgumentException("Slug is not valid. " +
                                        "It may only contain alphanumeric characters, +, -, ., or _");
        }

        var suffix = string.Empty;
        var suffixLength = InitSuffixLength;
        if (entity is Project)
        {
            suffix = GenerateRandomString(suffixLength);
        }

        foreach (var it in Enumerable.Range(0, 50))
        {
            // if it takes to long, increase the suffix length
            if (it % 5 == 0) suffixLength++;

            var finalSlug = requestedSlug + (string.IsNullOrEmpty(suffix) ? string.Empty : "-" + suffix);
            if (entity is Project && await slugRepository.IsProjectSlugTakenAsync(finalSlug))
            {
                suffix = GenerateRandomString(suffixLength);
                continue;
            }

            if (entity is IUniqueEntityWithParent child)
            {
                if (child.ParentId is null)
                {
                    throw new InvalidOperationException($"ParentId of entity with id {entity} is null");
                }

                var taken = await slugRepository.IsSlugTakenGivenParent(child, finalSlug, child.ParentId!.Value);
                if (taken) continue;
            }

            var slug = new Slug()
            {
                Value = finalSlug,
                EntityId = entity.Id,
                Type = SlugUtil.MapTypeToSlug(entity.GetType())
            };
            
            await slugRepository.AddAsync(slug);
            await slugRepository.SetDeprecatedButAsync(finalSlug, entity.Id);
            return finalSlug;
        }
        
        throw new InvalidOperationException($"Could not generate a unique slug for entity " +
                                            $"with id {entity.Id} in 50 tries.");
    }

    public async Task<Project?> ResolveProjectBySlugAsync(string slug)
    {
        var slugs = (await slugRepository.GetMatchingSlugs(slug, SlugType.Project)).ToList();
        if (slugs.Any())
        {
            return await projectRepository.GetByIdAsync(slugs.Single().EntityId);
        }
        return null;
    }

    public async Task<TEntity?> ResolveEntityWithParentBySlugAsync<TEntity>(string slug, Guid parentId)
        where TEntity : class, IUniqueEntityWithParent
    {
        var slugType = SlugUtil.MapTypeToSlug<TEntity>();
        var slugs = (await slugRepository.GetMatchingSlugs(slug, slugType)).ToList();
        if (!slugs.Any())
        {
            return null;
        }

        // this can be nicer, using a generic IQueryableByParenRepository
        List<TEntity> entities;
        if (typeof(TEntity) == typeof(DataCollectionEntity))
        {
            entities = (await collectionRepository.GetAllInProject(parentId)).Cast<TEntity>().ToList();
        }
        else if (typeof(TEntity) == typeof(DataStore))
        {
            entities = (await dataStoreRepository.GetAllInProject(parentId)).Cast<TEntity>().ToList();
        }
        else if (typeof(TEntity) == typeof(DataSet))
        {
            entities = (await dataSetRepository.GetByCollectionIdAsync(parentId)).Cast<TEntity>().ToList();
        }
        else
        {
            throw new InvalidOperationException($"Type {typeof(TEntity)} is not supported.");
        }

        var query = from e in entities
            join s in slugs on e.Id equals s.Id
            select e;

        return (TEntity) query.Single();
    }

    public async Task<IEnumerable<Slug>> GetSlugsForEntityAsync<TEntity>(Guid entityId)
        where TEntity : class, IUniqueEntity
    {
        var slugs = (await slugRepository.GetSlugsForEntity(entityId)).ToList();
        var slugType = SlugUtil.MapTypeToSlug<TEntity>();
        if (slugs.Any(s => s.Type != slugType))
        {
            throw new InvalidOperationException($"Entity with id {entityId} has slugs of wrong type" +
                                                $"but expected type {slugType}");
            
        }
        return slugs;
    }

    public async Task<Dictionary<Guid, IEnumerable<Slug>>> GetSlugsForEntitiesAsync<TEntity>(IEnumerable<Guid> entityIds)
        where TEntity : class, IUniqueEntity
    {
        var entityIdsList = entityIds.ToList();
        var slugType = SlugUtil.MapTypeToSlug<TEntity>();
    
        var allSlugs = await slugRepository.GetSlugsForEntities(entityIdsList, slugType);
    
        var slugsByEntity = allSlugs
            .GroupBy(s => s.EntityId)
            .ToDictionary(g => g.Key, g => g.AsEnumerable());
    
        // Validate all slugs have correct type
        if (slugsByEntity.Values.SelectMany(s => s).Any(s => s.Type != slugType))
        {
            throw new InvalidOperationException($"Some entities have slugs of wrong type, expected type {slugType}");
        }
    
        // Ensure all entity IDs have an entry (even if empty)
        foreach (var entityId in entityIdsList.Where(id => !slugsByEntity.ContainsKey(id)))
        {
            slugsByEntity[entityId] = Enumerable.Empty<Slug>();
        }
    
        return slugsByEntity;
    }

    private static string GenerateRandomString(int? suffixLength)
    {
        var randomString = new string(Enumerable.Range(0, suffixLength ?? InitSuffixLength)
            .Select(_ => AlphanumericChars[Random.Next(AlphanumericChars.Length)])
            .ToArray());
        return randomString;
    }
}