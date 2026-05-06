using RDPMS.Core.Persistence;

namespace RDPMS.Core.Server.Services.MetadataProjection;

public interface IEntityMetadataProjectionService
{
    Task RefreshAsync(
        IUniqueEntity entity,
        string metadataKey,
        CancellationToken cancellationToken = default);
}
