using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

[AutoRegister]
public class DataSetCreateRequestDTOMapper : IImportMapper<DataSet, DataSetCreateRequestDTO>
{
    public DataSet Import(DataSetCreateRequestDTO foreign)
    {
        if (foreign.Name is null)
        {
            throw new ArgumentException("Name is required.");
        }

        if (foreign.CreatedStampUTC is null)
        {
            throw new ArgumentException("CreatedStampUTC is required.");
        }

        if (foreign.CollectionId is null)
        {
            throw new ArgumentException("CollectionId is required.");
        }

        if (foreign.Slug is not null && !SlugUtil.IsValidSlug(foreign.Slug))
        {
            throw new ArgumentException("Slug is not valid.");
        }

        return new DataSet(foreign.Name)
        {
            Id = Guid.NewGuid(),
            Slug = foreign.Slug,
            ParentCollectionId = foreign.CollectionId.Value,
            AncestorDatasetIds = new List<Guid>(),
            AssignedTags = new List<Tag>(),
            CreatedStamp = foreign.CreatedStampUTC.Value,
            DeletedStamp = null,
            LifecycleState = DataSetState.Uninitialized,
            DeletionState = DeletionState.Active,
            CreateJob = null,
            Files = new List<DataFile>(),
            SourceForJobs = new List<Job>(),
            MetadataJsonFields = new List<DataEntityMetadataJsonField>()
        };
    }

    public IEnumerable<CheckSet<DataSetCreateRequestDTO>> ImportChecks()
    {
        throw new NotImplementedException();
    }
}
