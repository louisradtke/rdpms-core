using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.MetadataProjection;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Server.Services.MetadataProjection;

public sealed class EntityMetadataProjectionService(
    DbContext context,
    CachedMetadataProjectionRegistry registry,
    IMetadataDocumentReader documentReader,
    IServiceProvider serviceProvider,
    ILogger<EntityMetadataProjectionService> logger)
    : IEntityMetadataProjectionService
{
    public async Task RefreshAsync(
        IUniqueEntity entity,
        string metadataKey,
        CancellationToken cancellationToken = default)
    {
        var descriptors = registry.GetForEntity(entity.GetType(), metadataKey);
        foreach (var descriptor in descriptors)
        {
            await RefreshProjectionAsync(entity, descriptor, cancellationToken);
        }

        if (descriptors.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task RefreshProjectionAsync(
        IUniqueEntity entity,
        CachedMetadataProjectionDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        var writer = new MetadataProjectionWriter(descriptor);
        var strategy = GetStrategy(descriptor);
        var sourceLink = await QuerySourceLink(entity, descriptor, cancellationToken);

        JsonDocument? document = null;
        Exception? sourceReadException = null;
        try
        {
            MetadataProjectionSource source;
            if (sourceLink is null)
            {
                source = MetadataProjectionSource.Missing;
            }
            else
            {
                try
                {
                    document = await documentReader.ReadJsonDocumentAsync(sourceLink.Field, cancellationToken);
                }
                catch (Exception ex)
                {
                    sourceReadException = ex;
                    logger.LogWarning(
                        ex,
                        "Failed to read metadata source {MetadataKey} for projection {ProjectionId} on {EntityType} {EntityId}.",
                        descriptor.SourceKey,
                        descriptor.Id,
                        descriptor.EntityType.Name,
                        entity.Id);
                }

                source = new MetadataProjectionSource(
                    true,
                    document?.RootElement,
                    sourceLink.FieldId,
                    sourceLink.Field.UpdatedStamp);
            }

            await strategy.RefreshAsync(entity, source, writer, cancellationToken);
            writer.SetRefreshedAt(entity, DateTime.UtcNow);
            writer.SetSourceMetadataFieldId(entity, source.MetadataFieldId);
            writer.SetSourceStamp(entity, source.SourceStamp);
            writer.SetVersion(entity, strategy.Version);
            writer.SetError(entity, sourceReadException?.Message);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to refresh metadata projection {ProjectionId} for {EntityType} {EntityId}.",
                descriptor.Id,
                descriptor.EntityType.Name,
                entity.Id);

            writer.SetRefreshedAt(entity, DateTime.UtcNow);
            writer.SetSourceMetadataFieldId(entity, sourceLink?.FieldId);
            writer.SetSourceStamp(entity, sourceLink?.Field.UpdatedStamp);
            writer.SetVersion(entity, strategy.Version);
            writer.SetError(entity, ex.Message);
        }
        finally
        {
            document?.Dispose();
        }
    }

    private IMetadataProjectionStrategy GetStrategy(CachedMetadataProjectionDescriptor descriptor)
    {
        var strategy = ActivatorUtilities.GetServiceOrCreateInstance(
            serviceProvider,
            descriptor.StrategyType);

        if (strategy is not IMetadataProjectionStrategy metadataProjectionStrategy)
        {
            throw new InvalidOperationException(
                $"Projection strategy '{descriptor.StrategyType.FullName}' does not implement {nameof(IMetadataProjectionStrategy)}.");
        }

        if (!metadataProjectionStrategy.EntityType.IsAssignableFrom(descriptor.EntityType))
        {
            throw new InvalidOperationException(
                $"Projection strategy '{descriptor.StrategyType.FullName}' does not handle '{descriptor.EntityType.FullName}'.");
        }

        return metadataProjectionStrategy;
    }

    private async Task<DataEntityMetadataJsonField?> QuerySourceLink(
        IUniqueEntity entity,
        CachedMetadataProjectionDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        if (descriptor.SourceScope != MetadataSourceScope.Self)
        {
            throw new NotSupportedException($"Metadata source scope '{descriptor.SourceScope}' is not supported yet.");
        }

        var normalizedKey = descriptor.SourceKey.ToLowerInvariant();
        var query = context.Set<DataEntityMetadataJsonField>()
            .Include(l => l.Field)
            .ThenInclude(f => f.Value)
            .ThenInclude(f => f!.References)
            .Include(l => l.Field)
            .ThenInclude(f => f.ValidatedSchemas)
            .Where(l => l.MetadataKey == normalizedKey);

        query = entity switch
        {
            DataSet dataSet => query.Where(l => l.DataSetId == dataSet.Id),
            DataFile dataFile => query.Where(l => l.DataFileId == dataFile.Id),
            _ => throw new ArgumentException($"Unsupported metadata projection entity type '{entity.GetType().FullName}'.")
        };

        return await query.SingleOrDefaultAsync(cancellationToken);
    }
}
