using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

public class DataSetSummaryDTOMapper
    : IImportMapper<DataSet, DataSetSummaryDTO>,
    IExportMapper<DataSet, DataSetSummaryDTO>
{
    private readonly List<CheckSet<DataSetSummaryDTO>> _checkSets;
    public DataSetSummaryDTOMapper()
    {
        _checkSets = new();
        _checkSets.Add(CheckSet<DataSetSummaryDTO>.CreateErr(
            dto => dto.Id != null, _ => "Id may not be set. It will be set by the server."));
        _checkSets.Add(CheckSet<DataSetSummaryDTO>.CreateErr(
            dto => dto.Name == null, _ => "Name is required."));
        _checkSets.Add(CheckSet<DataSetSummaryDTO>.CreateErr(
            dto => dto.CreatedStampUTC == null, _ => "CreatedStampUTC is required."));
        _checkSets.Add(CheckSet<DataSetSummaryDTO>.CreateWarn(
            dto => dto.BeginStampUTC == null, 
            _ => "BeginStampUTC is auto-generated by the server, ignoring it."));
        _checkSets.Add(CheckSet<DataSetSummaryDTO>.CreateWarn(
            dto => dto.EndStampUTC == null, 
            _ => "EndStampUTC is auto-generated by the server, ignoring it."));
    }
    
    public DataSet Import(DataSetSummaryDTO foreign)
    {
        foreach (var checkSet in _checkSets)
        {
            if (checkSet.CheckFunc(foreign))
                continue;
            
            throw new ArgumentException(checkSet.MessageFunc(foreign));
        }

        var tags = new List<Tag>();
        
        var dataSet = new DataSet(foreign.Name ?? throw new IllegalStateException("Name is required."))
        {
            Id = foreign.Id ?? Guid.NewGuid(),
            AncestorDatasetIds = new List<Guid>(), // Initializing empty, as no information given
            AssignedTags = tags,
            CreatedStamp = foreign.CreatedStampUTC ?? throw new IllegalStateException("CreatedStampUTC is required."),
            DeletedStamp = foreign.DeletedStampUTC,
            State = foreign.State.HasValue
                ? (DataSetState)foreign.State.Value
                : DataSetState.Uninitialized,
            CreateJob = null, // Since DTO provides no information here
            Files = new List<DataFile>(), // Initializing empty as no information from DTO
            SourceForJobs = new List<Job>(), // Again, initialize empty
            MetadataJsonFields = new List<MetadataJsonField>() // Also initializing empty
        };

        return dataSet;
    }

    public IEnumerable<CheckSet<DataSetSummaryDTO>> ImportChecks() => _checkSets;


    public DataSetSummaryDTO Export(DataSet domain)
    {
        return new DataSetSummaryDTO
        {
            Id = domain.Id,
            Name = domain.Name,
            AssignedTags = domain.AssignedTags?.Select(tag => new TagDTO
            {
                Id = tag.Id,
                Name = tag.Name,
            }).ToList(),
            CreatedStampUTC = domain.CreatedStamp,
            DeletedStampUTC = domain.DeletedStamp,
            BeginStampUTC = domain.Files.OrderBy(f => f.CreatedStamp).FirstOrDefault()?.CreatedStamp,
            EndStampUTC = domain.Files.OrderByDescending(f => f.CreatedStamp).FirstOrDefault()?.CreatedStamp,
            State = (DataSetStateDTO)domain.State,
            IsTimeSeries = domain.Files.Any(file => file.BeginStamp.HasValue),
            IsDeleted = domain.DeletedStamp.HasValue,
            MetadataFields = domain.MetadataJsonFields.Select(f => f.Key).ToList()
        };
    }
}
