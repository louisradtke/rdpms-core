using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Model.Repositories;

namespace RDPMS.Core.Server.Services;

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

    public async Task<FileUploadTarget> RequestFileUploadAsync(DataFile file)
    {
        var target = await _dataFileRepository.AddFileAsync(file);
        if (target == null) throw new IllegalStateException();
        return target;
    }
}