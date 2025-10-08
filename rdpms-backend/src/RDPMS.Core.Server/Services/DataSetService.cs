using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services.Infra;

namespace RDPMS.Core.Server.Services;

public class DataSetService(
    IDataSetRepository repo) : GenericCollectionService<DataSet>(repo),
    IDataSetService
{
    public Task<IEnumerable<DataSet>> GetByCollectionAsync(Guid collectionId)
    {
        return repo.GetByCollectionIdAsync(collectionId);
    }

    public async Task UpdateFieldsAsync(Guid id, DataSet updates)
    {
        var existing = await repo.GetByIdAsync(id);
        if (existing == null) throw new KeyNotFoundException($"DataSet {id} not found");
        
        // Apply business rules for what can be updated
        existing.Name = updates.Name;
        existing.State = updates.State;
        // Don't update server-managed fields
        
        // Handle tags if provided
        
        await repo.UpdateFieldsAsync(existing);
    }
}
