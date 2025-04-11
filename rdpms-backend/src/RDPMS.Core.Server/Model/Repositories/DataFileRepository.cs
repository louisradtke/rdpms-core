using CommandLine;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories.Infra;
using KeyNotFoundException = RDPMS.Core.Infra.Exceptions.KeyNotFoundException;

namespace RDPMS.Core.Server.Model.Repositories;

public class DataFileRepository(RDPMSPersistenceContext ctx)
    : GenericRepository<DataFile>(ctx, ctx.DataFiles),
    IDataFileRepository
{
    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        var storeEntity = await ctx.DataStores.FindAsync(storeId);
        if (storeEntity == null) throw new KeyNotFoundException($"store with id {storeId} not found");

        await ctx.Entry(storeEntity).Reference(s => s.DataFiles).LoadAsync();
        return storeEntity.DataFiles;
    }
}