using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Logic;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class FileCreateRequestDTOMapper : IImportMapper<DataFile, FileCreateRequestDTO, ContentType>
{
    public DataFile Import(FileCreateRequestDTO foreign, ContentType arg)
    {
        if (foreign.Name == null || foreign.Size == null || foreign.CreatedStamp == null || foreign.Hash == null)
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
            SizeBytes = foreign.Size.Value,
            SHA256Hash = foreign.Hash,
            CreatedStamp = foreign.CreatedStamp.Value,
            BeginStamp = foreign.BeginStamp,
            EndStamp = foreign.EndStamp
        };
    }

    public IEnumerable<CheckSet<FileCreateRequestDTO>> ImportChecks() => [];
}