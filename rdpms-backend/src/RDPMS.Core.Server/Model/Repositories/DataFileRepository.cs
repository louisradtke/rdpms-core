using CommandLine;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories.Infra;
using KeyNotFoundException = RDPMS.Core.Infra.Exceptions.KeyNotFoundException;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataFileRepository(DbContext ctx)
    : GenericRepository<DataFile>(ctx, files => files
            .Include(f => f.Locations)
            .Include(f => f.FileType)),
    IDataFileRepository
{
    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        var storeEntity = await Context.Set<DataStore>()
            .AsNoTracking()
            .Include(s => s.DataFiles)
            .ThenInclude(f => f.Locations)
            .Include(s => s.DataFiles)
            .ThenInclude(f => f.FileType)
            .FirstOrDefaultAsync(s => s.Id == storeId);
        if (storeEntity == null) throw new KeyNotFoundException($"store with id {storeId} not found");

        return storeEntity.DataFiles;
    }
}