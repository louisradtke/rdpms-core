namespace RDPMS.Core.Persistence.MetadataProjection;

public interface IMetadataProjectionStrategy
{
    Type EntityType { get; }
    string Version { get; }

    ValueTask RefreshAsync(
        object entity,
        MetadataProjectionSource source,
        MetadataProjectionWriter writer,
        CancellationToken cancellationToken);
}

public interface IMetadataProjectionStrategy<TEntity> : IMetadataProjectionStrategy
{
    ValueTask RefreshAsync(
        TEntity entity,
        MetadataProjectionSource source,
        MetadataProjectionWriter writer,
        CancellationToken cancellationToken);
}

public abstract class MetadataProjectionStrategy<TEntity> : IMetadataProjectionStrategy<TEntity>
{
    public Type EntityType => typeof(TEntity);
    public abstract string Version { get; }

    public abstract ValueTask RefreshAsync(
        TEntity entity,
        MetadataProjectionSource source,
        MetadataProjectionWriter writer,
        CancellationToken cancellationToken);

    public ValueTask RefreshAsync(
        object entity,
        MetadataProjectionSource source,
        MetadataProjectionWriter writer,
        CancellationToken cancellationToken)
    {
        if (entity is not TEntity typedEntity)
        {
            throw new InvalidOperationException(
                $"Projection strategy for '{typeof(TEntity).FullName}' cannot refresh '{entity.GetType().FullName}'.");
        }

        return RefreshAsync(typedEntity, source, writer, cancellationToken);
    }
}
