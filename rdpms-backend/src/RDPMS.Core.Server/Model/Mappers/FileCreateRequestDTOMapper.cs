using System.Text.RegularExpressions;
using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class FileCreateRequestDTOMapper : IImportMapper<DataFile, FileCreateRequestDTO, ContentType>
{
    private IEnumerable<CheckSet<FileCreateRequestDTO>> _importChecks;

    public FileCreateRequestDTOMapper()
    {
        _importChecks = [
            CheckSet<FileCreateRequestDTO>.CreateErrCond(
                dto => CheckIfPathIsValid(dto.Name), _ => "Name cannot contain '..'")
        ];
    }

    public DataFile Import(FileCreateRequestDTO foreign, ContentType arg)
    {
        foreach (var checkSet in _importChecks)
        {
            if (checkSet.CheckFunc(foreign) || (int)checkSet.Severity <= (int)ErrorSeverity.Error)
                continue;
            
            throw new ArgumentException(checkSet.MessageFunc(foreign));
        }

        if (foreign.Name == null || foreign.SizeBytes == null ||
            foreign.CreatedStamp == null || foreign.PlainSHA256Hash == null)
        {
            throw new ArgumentException(
                "Name, Size, CreatedStamp, and Hash are required fields in DataFileCreateRequestDTO");
        }
        
        if ((foreign.BeginStamp == null) != (foreign.EndStamp == null))
        {
            throw new ArgumentException("BeginStamp and EndStamp must be both null or both non-null.");
        }

        return new DataFile(name: foreign.Name)
        {
            FileType = arg,
            SizeBytes = foreign.SizeBytes.Value,
            SHA256Hash = foreign.PlainSHA256Hash,
            CreatedStamp = foreign.CreatedStamp.Value,
            BeginStamp = foreign.BeginStamp,
            EndStamp = foreign.EndStamp
        };
    }

    public IEnumerable<CheckSet<FileCreateRequestDTO>> ImportChecks() => _importChecks;

    public static bool CheckIfPathIsValid(string? path)
    {
        // TODO: this should not be done manually!
        if (string.IsNullOrWhiteSpace(path)) return false;
        var invalidChars = Path.GetInvalidPathChars().Concat(['\\', '~']).ToArray();
        var invalidComponents = new[] { ".", ".." };
        if (path.Any(c => invalidChars.Contains(c))) return false;
        if (path.Split('/').Any(c => invalidComponents.Contains(c) || string.IsNullOrWhiteSpace(c))) return false;
        
        return !Path.IsPathRooted(path);
    }
}