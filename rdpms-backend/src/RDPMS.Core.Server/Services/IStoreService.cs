using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

/// <summary>
/// The purpose of this service is to manage instances of DataStores.
/// It is not meant to be used to manage instances of <see cref="FileStorageReference"/>.
/// For each data store type, there should be a corresponding service type.
/// For S3, see <see cref="IS3Service"/>.
/// </summary>
public interface IStoreService : IReadonlyGenericCollectionService<DataStore>
{
    Task AddAsync(DataStore item);
}