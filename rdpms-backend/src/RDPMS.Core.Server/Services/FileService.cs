using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Model.Services;

public class FileService : IFileService
{
    private readonly DataFileRepository _dataFileRepository;

    public FileService(DataFileRepository dataFileRepository)
    {
        _dataFileRepository = dataFileRepository;
    }

    public async Task<IEnumerable<DataFile>> GetFilesAsync()
    {
        return await _dataFileRepository.GetFilesAsync();
    }

    public async Task<DataFile> GetFileAsync(Guid id)
    {
        return await _dataFileRepository.GetFileAsync(id);
    }

    public async Task<IEnumerable<DataFile>> GetFilesInStoreAsync(Guid storeId)
    {
        return await _dataFileRepository.GetFilesInStoreAsync(storeId);
    }
}