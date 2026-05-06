using System.Text;
using System.Text.Json;
using Asp.Versioning;
using Corvus.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.QueryEngine;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server.Controllers.V1;

[ApiController]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/data/datasets")]
[ApiVersion("1.0")]
public class DataSetsController(
    IDataSetService dataSetService,
    IFileService fileService,
    IContentTypeService typeService,
    IDataCollectionEntityService collectionService,
    DataSetSummaryDTOMapper dataSetSummaryMapper,
    DataSetDetailedDTOMapper dataSetDetailedMapper,
    FileSummaryDTOMapper fileSummaryMapper,
    IS3Service s3Service,
    IStoreService storeService,
    IMetadataService metadataService,
    IContentTypeService contentTypeService,
    IImportMapper<DataSet, DataSetCreateRequestDTO> dataSetCreateReqMapper,
    IImportMapper<DataFile, S3FileCreateRequestDTO, ContentType> s3dfCreateReqMapper,
    IExportMapper<MetadataJsonField, MetaDateDTO> metadataMapper,
    ILogger<DataSetsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Query data sets.
    /// </summary>
    /// <param name="collectionId"></param>
    /// <param name="deleted">comma-separated list of strings, case-insensitive.
    /// Default is <see cref="DeletionState.Active"/>
    /// Valid values can be found in <see cref="DeletionState"/>.</param>
    /// <param name="view">Whether to only return dataset summaries (default), or metadata as well.</param>
    /// <param name="metadataTarget">If view is set to yield metadata,
    /// they will be set either on datasets or files.</param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType<IEnumerable<DataSetSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataSetSummaryDTO>>> Get(
        [FromQuery] Guid? collectionId = null,
        [FromQuery] string? deleted = null,
        [FromQuery] DataSetListViewMode view = DataSetListViewMode.Summary,
        [FromQuery] MetadataColumnTargetDTO metadataTarget = MetadataColumnTargetDTO.Dataset
    )
    {
        var datasetsQuery = dataSetService.Query();

        try
        {
            datasetsQuery = QueryDatasets(datasetsQuery, collectionId, deleted);
        }
        catch (QueryException ex)
        {
            return BadRequest(new ErrorMessageDTO() { Message = ex.PublicMessage });
        }

        var datasets = await datasetsQuery.ToListAsync();
        if (view == DataSetListViewMode.Summary)
        {
            var summaryDtos = datasets
                .Select(dataSetSummaryMapper.Export)
                .ToList();
            return Ok(summaryDtos);
        }

        var dtos = await QueryAndBuildDatasetDtos(metadataTarget, datasets);

        return Ok(dtos);
    }

    /// <summary>
    /// Query datasets, with additional metadata-based query.
    /// </summary>
    /// <param name="collectionId"></param>
    /// <param name="deleted">comma-separated list of strings, case-insensitive.
    /// Default is <see cref="DeletionState.Active"/>
    /// Valid values can be found in <see cref="DeletionState"/>.</param>
    /// <param name="view">Whether to only return dataset summaries (default), or metadata as well.</param>
    /// <param name="metadataTarget">If view is set to yield metadata,
    /// they will be set either on datasets or files.</param>
    /// <param name="query">Query over metadata items.</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType<IEnumerable<DataSetSummaryDTO>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DataSetSummaryDTO>>> QueryComplex(
        [FromBody] MetadataQueryDTO query,
        [FromQuery] Guid? collectionId = null,
        [FromQuery] string? deleted = null,
        [FromQuery] DataSetListViewMode view = DataSetListViewMode.Summary,
        [FromQuery] MetadataColumnTargetDTO metadataTarget = MetadataColumnTargetDTO.Dataset
    )
    {
        var datasetsQuery = dataSetService.Query();

        List<DataSet> datasets;
        try
        {
            datasetsQuery = QueryDatasets(datasetsQuery, collectionId, deleted);
            datasets = (await FilterByMetadata(await datasetsQuery.ToListAsync(), query)).ToList();
        }
        catch (QueryException ex)
        {
            return BadRequest(new ErrorMessageDTO() { Message = ex.PublicMessage });
        }

        if (view == DataSetListViewMode.Summary)
        {
            var summaryDtos = datasets
                .Select(dataSetSummaryMapper.Export)
                .ToList();
            return Ok(summaryDtos);
        }

        var dtos = await QueryAndBuildDatasetDtos(metadataTarget, datasets);

        return Ok(dtos);
    }

    private async Task<IEnumerable<DataSet>> FilterByMetadata(IEnumerable<DataSet> datasets, MetadataQueryDTO query)
    {
        var datasetArray = datasets.ToArray();
        var engine = ObjectQueryEngine.CreateDefault();
        var astDict = query.Queries
            .Where(q => q.Target == MetadataColumnTargetDTO.Dataset)
            .ToDictionary(
                q => q.MetadataKey,
                q =>
                {
                    var singleQuery = new ObjectQueryDslV1Schema(q.Query);
                    if (!singleQuery.IsValid()) throw new QueryException($"Invalid query schema for {q.MetadataKey}");
                    return engine.ParseToAst(singleQuery);
                }
            );

        var metadataDict = new Dictionary<Guid, string>();
        var storesCache = new Dictionary<Guid, S3DataStore>();
        foreach (var field in datasetArray.SelectMany(ds => ds.MetadataJsonFields))
        {
            if (field.DataSetId is null) continue;
            if (field.Field.Value is null) continue;
            var reference = field.Field.Value.References
                .FirstOrDefault(r => r.StorageType == StorageType.Db);
            if (reference is DbFileStorageReference dbReference)
            {
                var data  = dbReference.Data;
                metadataDict[field.FieldId] = Encoding.UTF8.GetString(data);
                continue;
            }

            reference = field.Field.Value.References
                .FirstOrDefault(r => r.StorageType == StorageType.S3);
            if (reference is not S3FileStorageReference s3Reference) continue;
            if (reference.StoreFid is null) continue;
            if (!storesCache.TryGetValue(reference.StoreFid.Value, out var store))
            {
                store = await storeService.GetByIdAsync(reference.StoreFid.Value) as S3DataStore
                        ?? throw new InvalidOperationException();
                storesCache[reference.StoreFid.Value] = store;
            }

            var bytes = await s3Service.GetFileAsync(s3Reference, store);
            metadataDict[field.FieldId] = Encoding.UTF8.GetString(bytes);
        }
        
        Func<IEnumerable<MetadataQueryPartDTO>, Func<MetadataQueryPartDTO, bool>, bool> predicate = query.Mode switch
        {
            QueryMode.And => Enumerable.All,
            QueryMode.Or => Enumerable.Any,
            _ => throw new ArgumentOutOfRangeException()
        };

        return datasetArray.Where(ds =>
        {
            return predicate(query.Queries, part =>
            {
                if (!ds.Metadata.TryGetValue(part.MetadataKey, out var field)) return false;
                // astDict[part.MetadataKey].
                var jsonElem = JsonDocument.Parse(metadataDict[field.Id]).RootElement;
                return engine.IsMatch(astDict[part.MetadataKey], jsonElem);
            });
        });
    }
    
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DataSetSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DataSetSummaryDTO>> GetById([FromRoute] Guid id)
    {
        var dto = await QueryDataSetDetailedDTO(id);
        return Ok(dto);
    }

    private async Task<DataSetSummaryDTO> QueryDataSetDetailedDTO(Guid id)
    {
        var domainItem = await dataSetService.GetByIdAsync(id);
        var dto = dataSetDetailedMapper.Export(domainItem);

        var fileIds = domainItem.Files.Select(f => f.Id).Distinct().ToList();
        var validatedFileMetaDates = fileIds.Count > 0
            ? await fileService.GetValidatedMetadates(fileIds)
            : new Dictionary<Guid, List<string>>();

        if (dto.Files is not null)
        {
            foreach (var file in dto.Files)
            {
                file.DownloadURI = fileService.GetContentApiUri(file.Id!.Value, HttpContext);
                var fileDomain = domainItem.Files.Single(f => f.Id == file.Id!.Value);
                validatedFileMetaDates.TryGetValue(fileDomain.Id, out var fileMetadateList);
                file.MetaDates = fileDomain.MetadataJsonFields
                    .Select(f => new AssignedMetaDateDTO
                    {
                        MetadataKey = f.MetadataKey,
                        MetadataId = f.FieldId,
                        CollectionSchemaVerified = fileMetadateList?.Contains(f.MetadataKey) ?? false
                    })
                    .ToList();
            }
        }

        var validatedMetaDates = await dataSetService
            .GetValidatedMetadates([domainItem.Id]);

        validatedMetaDates.TryGetValue(domainItem.Id, out var datasetMetadataList);
        dto.MetaDates = domainItem.MetadataJsonFields
            .Select(f => new AssignedMetaDateDTO
            {
                MetadataKey = f.MetadataKey,
                MetadataId = f.FieldId,
                CollectionSchemaVerified = datasetMetadataList?.Contains(f.MetadataKey) ?? false
            })
            .ToList();
        return dto;
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> DeleteById([FromRoute] Guid id)
    {
        if (!await dataSetService.CheckForIdAsync(id))
        {
            return NotFound(new ErrorMessageDTO { Message = "no data set with that id" });
        }
        
        var ds = await dataSetService.GetByIdAsync(id);
        ds.DeletedStamp = DateTime.UtcNow;
        ds.DeletionState = DeletionState.DeletionPending;
        await dataSetService.UpdateAsync(ds);
        return Ok();
    }

    /// <summary>
    /// Add a single item to the system.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns>On success, responds with the guid of the new data set.</returns>
    [HttpPost("new")]
    [Consumes("application/json")]
    [ProducesResponseType<DataSetSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post([FromBody] DataSetCreateRequestDTO dto)
    {
        DataSet domainItem;
        try
        {
            domainItem = dataSetCreateReqMapper.Import(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }

        if (domainItem.ParentId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "ParentId (collection) is required." });
        }

        if (domainItem.Slug == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "Slug is required." });
        }

        var slugValidationResult = await dataSetService.ValidateSlug(domainItem.Slug, domainItem.ParentId.Value);
        switch (slugValidationResult)
        {
            case DataSetSlugValidationResult.Valid:
                break;
            case DataSetSlugValidationResult.InvalidFormat:
                return BadRequest(new ErrorMessageDTO { Message = SlugUtil.GetInvalidSlugMessage() });
            case DataSetSlugValidationResult.AlreadyTaken:
                return BadRequest(new ErrorMessageDTO
                {
                    Message = $"Slug '{domainItem.Slug}' is already taken in this collection."
                });
            default:
                throw new ArgumentOutOfRangeException();
        }

        await dataSetService.AddAsync(domainItem);

        var responseDto = await QueryDataSetDetailedDTO(domainItem.Id);
        return Ok(responseDto);
    }

    /// <summary>
    /// Register an already existing S3-backed dataset and seal it in a single operation.
    /// All object keys are relative to the referenced datastore prefix.
    /// </summary>
    /// <param name="dto">Dataset and file references to register.</param>
    /// <returns>The created dataset.</returns>
    [HttpPost("new/sealed/s3")]
    [Consumes("application/json")]
    [ProducesResponseType<DataSetSummaryDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> PostSealedS3([FromBody] SealedS3DataSetCreateRequestDTO dto)
    {
        var datasetValidationError = ValidateSealedS3DatasetRequest(dto);
        if (datasetValidationError is not null)
        {
            return BadRequest(new ErrorMessageDTO { Message = datasetValidationError });
        }

        if (!await collectionService.CheckForIdAsync(dto.CollectionId!.Value))
        {
            return BadRequest(new ErrorMessageDTO { Message = "CollectionId does not refer to an existing collection." });
        }

        if (!await storeService.CheckForIdAsync(dto.StoreId!.Value))
        {
            return BadRequest(new ErrorMessageDTO { Message = "StoreId does not refer to an existing store." });
        }

        var collection = await collectionService.GetByIdAsync(dto.CollectionId.Value);
        var requestedStore = await storeService.GetByIdAsync(dto.StoreId!.Value);
        if (requestedStore is not S3DataStore store)
        {
            return BadRequest(new ErrorMessageDTO { Message = "StoreId must refer to an S3 data store." });
        }

        if (collection.ParentProjectId != store.ParentProjectId)
        {
            return BadRequest(new ErrorMessageDTO
            {
                Message = "CollectionId and StoreId must belong to the same project."
            });
        }

        var slug = dto.Slug!.Trim();
        var slugValidationResult = await dataSetService.ValidateSlug(slug, dto.CollectionId.Value);
        switch (slugValidationResult)
        {
            case DataSetSlugValidationResult.Valid:
                break;
            case DataSetSlugValidationResult.InvalidFormat:
                return BadRequest(new ErrorMessageDTO { Message = SlugUtil.GetInvalidSlugMessage() });
            case DataSetSlugValidationResult.AlreadyTaken:
                return BadRequest(new ErrorMessageDTO
                {
                    Message = $"Slug '{slug}' is already taken in this collection."
                });
            default:
                throw new ArgumentOutOfRangeException();
        }

        List<DataFile> files;
        try
        {
            files = await BuildSealedS3DataFiles(dto, store);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }

        foreach (var reference in files.SelectMany(f => f.References).OfType<S3FileStorageReference>())
        {
            bool isValid;
            try
            {
                isValid = await s3Service.ValidateFileRefAsync(reference, store);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorMessageDTO { Message = ex.Message });
            }

            if (!isValid)
            {
                return BadRequest(new ErrorMessageDTO
                {
                    Message = $"Could not validate object '{reference.ObjectKey}' in store '{store.Id}'."
                });
            }
        }

        var domainItem = new DataSet(dto.Name!.Trim())
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            ParentCollectionId = dto.CollectionId.Value,
            AncestorDatasetIds = [],
            AssignedTags = [],
            CreatedStamp = dto.CreatedStampUTC!.Value,
            DeletedStamp = null,
            LifecycleState = DataSetState.Sealed,
            DeletionState = DeletionState.Active,
            Files = files,
            SourceForJobs = [],
            MetadataJsonFields = []
        };

        foreach (var file in domainItem.Files)
        {
            file.ParentDataSetId = domainItem.Id;
        }

        await dataSetService.AddAsync(domainItem);

        var responseDto = await QueryDataSetDetailedDTO(domainItem.Id);
        return Ok(responseDto);
    }

    /// <summary>
    /// Add a single file to the system. Request a single S3 upload URL.
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="storeId"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("{id:guid}/add/s3")]
    [Consumes("application/json")]
    [ProducesResponseType<FileCreateResponseDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<FileCreateResponseDTO>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> PostAddS3([FromRoute] Guid id, [FromBody] S3FileCreateRequestDTO requestDto)
    {
        if (requestDto.ContentTypeId == null)
        {
            return BadRequest(new ErrorMessageDTO { Message = "ContentTypeId is required." });
        }

        if (!await dataSetService.CheckForIdAsync(id))
        {
            return BadRequest(new ErrorMessageDTO { Message = "there is no data set for the given id." });
        }

        if (!await typeService.CheckForIdAsync(requestDto.ContentTypeId.Value))
        {
            return BadRequest(new ErrorMessageDTO { Message = "there is no content type for the given id." });
        }
        var type = await typeService.GetByIdAsync(requestDto.ContentTypeId.Value);

        var dataset = await dataSetService.GetByIdAsync(id);
        if (dataset.ParentId == null)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ErrorMessageDTO { Message = "DataSet must have a parent collection." });
        }
        if (dataset.LifecycleState != DataSetState.Uninitialized)
        {
            return BadRequest(new ErrorMessageDTO { Message = $"DataSet must be in {nameof(DataSetState.Uninitialized)} state." });
        }

        S3DataStore? store;
        var collection = await collectionService.GetByIdAsync(dataset.ParentId!.Value);
        if (collection.DefaultDataStore is S3DataStore s3Store) store = s3Store;
        else return BadRequest(new ErrorMessageDTO
        {
            Message = "Collection must have a default S3 data store, or you must provide a storeId."
        });

        // S3DataStore? store;
        // if (storeId == null)
        // {
        //     var collection = await collectionService.GetByIdAsync(dataset.ParentId!.Value);
        //     if (collection.DefaultDataStore is S3DataStore s3Store) store = s3Store;
        //     else return BadRequest(new ErrorMessageDTO
        //     {
        //         Message = "Collection must have a default S3 data store, or you must provide a storeId."
        //     });
        // }
        // else
        // {
        //     var requestedStore = await storeService.GetByIdAsync(storeId.Value);
        //     if (requestedStore is not S3DataStore s3Store)
        //     {
        //         return BadRequest(new ErrorMessageDTO
        //         {
        //             Message = "StoreId must refer to an S3 data store."
        //         });
        //     }
        //     store = s3Store;
        // }

        // var url = s3Service.RequestPresignedUploadUrlAsync(store, file.Name);

        var requestedFile = s3dfCreateReqMapper.Import(requestDto, type);
        requestedFile.ParentDataSetId = id;
        if (dataset.Files.Any(f => f.Name == requestedFile.Name))
        {
            return BadRequest(new ErrorMessageDTO
            {
                Message = $"File with name {requestedFile.Name} is already registered."
            });
        }
        var reference = requestedFile.References.Single() as S3FileStorageReference ??
                        throw new InvalidOperationException();
        await fileService.AddAsync(requestedFile);
        var response = await fileService.RequestS3FileUploadAsync(
            requestedFile, reference, id, store.Id);
        var target = FileCreateResponseDTOMapper.ToDTO(response);

        return Ok(target);
    }

    private static string? ValidateSealedS3DatasetRequest(SealedS3DataSetCreateRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return "Name is required.";
        }

        if (dto.CreatedStampUTC is null)
        {
            return "CreatedStampUTC is required.";
        }

        if (dto.CollectionId is null)
        {
            return "CollectionId is required.";
        }

        if (dto.StoreId is null)
        {
            return "StoreId is required.";
        }

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            return "Slug is required.";
        }

        if (dto.Files is null || dto.Files.Count == 0)
        {
            return "At least one file is required.";
        }

        var duplicateName = dto.Files
            .Where(f => !string.IsNullOrWhiteSpace(f.Name))
            .GroupBy(f => f.Name!.Trim(), StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1)
            ?.Key;
        if (duplicateName is not null)
        {
            return $"File name '{duplicateName}' is duplicated in this dataset.";
        }

        var duplicateObjectKey = dto.Files
            .Where(f => !string.IsNullOrWhiteSpace(f.ObjectKey))
            .GroupBy(f => NormalizeObjectKey(f.ObjectKey!), StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1)
            ?.Key;
        if (duplicateObjectKey is not null)
        {
            return $"Object key '{duplicateObjectKey}' is duplicated in this dataset.";
        }

        return null;
    }

    private async Task<List<DataFile>> BuildSealedS3DataFiles(
        SealedS3DataSetCreateRequestDTO datasetDto,
        S3DataStore store)
    {
        var contentTypeCache = new Dictionary<Guid, ContentType>();
        var files = new List<DataFile>();

        foreach (var fileDto in datasetDto.Files!)
        {
            if (string.IsNullOrWhiteSpace(fileDto.Name))
            {
                throw new ArgumentException("File Name is required.");
            }

            if (string.IsNullOrWhiteSpace(fileDto.ObjectKey))
            {
                throw new ArgumentException($"ObjectKey is required for file '{fileDto.Name}'.");
            }

            if (fileDto.ContentTypeId is null)
            {
                throw new ArgumentException($"ContentTypeId is required for file '{fileDto.Name}'.");
            }

            if (fileDto.SizeBytes is null || fileDto.SizeBytes < 0)
            {
                throw new ArgumentException($"SizeBytes must be >= 0 for file '{fileDto.Name}'.");
            }

            if ((fileDto.BeginStampUTC is null) != (fileDto.EndStampUTC is null))
            {
                throw new ArgumentException(
                    $"BeginStampUTC and EndStampUTC must be both null or both non-null for file '{fileDto.Name}'.");
            }

            if (!contentTypeCache.TryGetValue(fileDto.ContentTypeId.Value, out var contentType))
            {
                if (!await typeService.CheckForIdAsync(fileDto.ContentTypeId.Value))
                {
                    throw new ArgumentException($"ContentTypeId does not exist for file '{fileDto.Name}'.");
                }

                contentType = await typeService.GetByIdAsync(fileDto.ContentTypeId.Value);
                contentTypeCache[fileDto.ContentTypeId.Value] = contentType;
            }

            var compression = ParseCompressionAlgorithm(fileDto.CompressionAlgorithm, fileDto.Name);
            var plainHash = fileDto.PlainSHA256Hash ?? string.Empty;
            var storageHash = fileDto.StoredSHA256Hash ?? plainHash;
            var objectKey = NormalizeObjectKey(fileDto.ObjectKey);
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                throw new ArgumentException($"ObjectKey is required for file '{fileDto.Name}'.");
            }

            files.Add(new DataFile(fileDto.Name.Trim())
            {
                FileType = contentType,
                SizeBytes = fileDto.SizeBytes.Value,
                SHA256Hash = plainHash,
                CreatedStamp = fileDto.CreatedStampUTC ?? datasetDto.CreatedStampUTC!.Value,
                BeginStamp = fileDto.BeginStampUTC,
                EndStamp = fileDto.EndStampUTC,
                DeletionState = DeletionState.Active,
                References =
                [
                    new S3FileStorageReference
                    {
                        StoreFid = store.Id,
                        ObjectKey = objectKey,
                        Algorithm = compression,
                        SizeBytes = fileDto.SizeBytes.Value,
                        SHA256Hash = storageHash
                    }
                ],
                MetadataJsonFields = []
            });
        }

        return files;
    }

    private static CompressionAlgorithm ParseCompressionAlgorithm(string? value, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return CompressionAlgorithm.Plain;
        }

        if (Enum.TryParse<CompressionAlgorithm>(value, ignoreCase: true, out var algorithm))
        {
            return algorithm;
        }

        throw new ArgumentException($"Unknown CompressionAlgorithm '{value}' for file '{fileName}'.");
    }

    private static string NormalizeObjectKey(string objectKey)
    {
        return objectKey.Trim().TrimStart('/');
    }

    /// <summary>
    /// Seals a data set. Only works for data sets that are in "Uninitialized" state.
    /// </summary>
    /// <param name="id">The data set id.</param>
    /// <returns>An error, </returns>
    [HttpPut("{id:guid}/seal")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SealDataset([FromRoute] Guid id)
    {
        if (!await dataSetService.CheckForIdAsync(id))
        {
            return NotFound("no data set with that id");
        }

        try
        {
            await dataSetService.SealDataset(id);
            return Ok();
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new ErrorMessageDTO() { Message = e.Message });
        }
    }

    /// <summary>
    /// Adds or sets meta documents for a data set.
    /// </summary>
    /// <param name="id">ID of the data set</param>
    /// <param name="key">Case-insensitive key of the meta date on the data set</param>
    /// <param name="value">JSON meta document</param>
    /// <returns>HTTP 200 if meta date was replaced successfully (key refers to new date).
    /// HTTP 201, if a new meta date was created.
    /// HTTP 400, if an input-related problem occurs.</returns>
    [HttpPut("{id:guid}/metadata/{key}")]
    [ProducesResponseType<MetaDateDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<MetaDateDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [Consumes("application/octet-stream", "application/json")]
    public async Task<ActionResult> AssignMetadate([FromRoute] Guid id, [FromRoute] string key,
        [FromBody] byte[] value)
    {
        DataSet dataSet;
        try
        {
            dataSet = await dataSetService.GetByIdAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorMessageDTO { Message = "No dataset with that id." });
        }
        
        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug." });

        var contentType = await contentTypeService.GetByMimeType(
            "application/json",
            dataSet.ParentCollection?.ParentId
                ?? throw new InvalidOperationException("DataSet must have a parent collection.")
        );

        var valueStr = Encoding.UTF8.GetString(value);
        var field = await metadataService.MakeFieldFromValue(valueStr, contentType);
        var modified = dataSet.MetadataJsonFields.Any(f =>
            f.MetadataKey.Equals(key, StringComparison.OrdinalIgnoreCase));

        await metadataService.AssignMetadate(dataSet, key, field);

        var returnDto = metadataMapper.Export(await metadataService.GetByIdAsync(field.Id));

        if (!modified) return CreatedAtAction(nameof(MetaDataController.GetMetadate), "MetaData",
            new { field.Id }, returnDto);
        return Ok(returnDto);
    }

    /// <summary>
    /// Rename key for metadate relation.
    /// </summary>
    /// <param name="id">The dataset ID.</param>
    /// <param name="key">The old key.</param>
    /// <param name="newKey">The new key.</param>
    /// <returns></returns>
    [HttpPost("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RenameMetadate([FromRoute] Guid id, [FromRoute] string key,
        [FromQuery] string newKey)
    {
        DataSet dataSet;
        try
        {
            dataSet = await dataSetService.GetByIdAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new ErrorMessageDTO { Message = ex.Message });
        }

        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for key."});
        if (!SlugUtil.IsValidSlug(newKey))
        {
            return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for newName."});
        }

        try
        {
            await metadataService.RenameMetadate(dataSet, key, newKey);
            return Ok();
        }
        catch (InvalidOperationException)
        {
            return BadRequest(new ErrorMessageDTO { Message = "No such metadata key." });
        }
    }

    /// <summary>
    /// Removes metadate relation with resp. key.
    /// </summary>
    /// <param name="id">ID of the dataset.</param>
    /// <param name="key">Key to remove the relation for</param>
    /// <returns></returns>
    [HttpDelete("{id:guid}/metadata/{key}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ErrorMessageDTO>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RemoveMetadate([FromRoute] Guid id, [FromRoute] string key)
    {
        DataSet dataSet;
        try
        {
            dataSet = await dataSetService.GetByIdAsync(id);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ErrorMessageDTO { Message = ex.Message });
        }

        if (!SlugUtil.IsValidSlug(key)) return BadRequest(new ErrorMessageDTO { Message = "Invalid slug for key."});

        var removed = await metadataService.RemoveMetadate(dataSet, key);
        if (!removed) return BadRequest(new ErrorMessageDTO { Message = "No such metadata key." });
        return Ok();
    }
        private static IQueryable<DataSet> QueryDatasets(IQueryable<DataSet> datasetsQuery,
        Guid? collectionId, string? deleted)
    {
        // filter for deletion state
        try
        {
            var queriedDeletionStates = deleted?
                .Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Select(s => Enum.Parse<DeletionState>(s, true))
                .ToArray();
            if (queriedDeletionStates is not null && queriedDeletionStates.Length > 0)
            {
                datasetsQuery = datasetsQuery
                    .Where(ds => queriedDeletionStates.Contains(ds.DeletionState));
            }
            else
            {
                datasetsQuery = datasetsQuery
                    .Where(ds => ds.DeletionState == DeletionState.Active);
            }
        }
        catch (ArgumentException ae)
        {
            throw new QueryException() { PublicMessage = $"Invalid value for deleted: {ae.Message}" };
        }

        if (collectionId is not null)
        {
            datasetsQuery = datasetsQuery.Where(ds => ds.ParentCollectionId == collectionId);
        }

        return datasetsQuery;
    }

    private async Task<List<DataSetSummaryDTO>> QueryAndBuildDatasetDtos(MetadataColumnTargetDTO metadataTarget, List<DataSet> datasets)
    {
        List<DataSetSummaryDTO> dtos;
        if (metadataTarget == MetadataColumnTargetDTO.File)
        {
            var fileIds = datasets
                .SelectMany(ds => ds.Files.Select(f => f.Id))
                .Distinct()
                .ToList();
            var validatedDataFileMetaDates = fileIds.Count > 0
                ? await fileService.GetValidatedMetadates(fileIds)
                : new Dictionary<Guid, List<string>>();

            dtos = datasets.Select<DataSet, DataSetSummaryDTO>(domain =>
            {
                var dto = dataSetSummaryMapper.Export(domain);
                dto.Files = domain.Files.Select(file =>
                {
                    validatedDataFileMetaDates.TryGetValue(file.Id, out var list);

                    var fileDto = fileSummaryMapper.Export(file);
                    fileDto.DownloadURI = fileService.GetContentApiUri(file.Id, HttpContext);
                    fileDto.MetaDates = file.MetadataJsonFields
                        .Select(f => new AssignedMetaDateDTO
                        {
                            MetadataKey = f.MetadataKey,
                            MetadataId = f.FieldId,
                            CollectionSchemaVerified = list?.Contains(f.MetadataKey) ?? false
                        })
                        .ToList();
                    return fileDto;
                }).ToList();
                return dto;
            }).ToList();
        }
        else
        {
            
            var validatedDataSetMetaDates = await dataSetService.GetValidatedMetadates(
                datasets.Select(ds => ds.Id).ToList());
            dtos = datasets.Select<DataSet, DataSetSummaryDTO>(domain =>
            {
                validatedDataSetMetaDates.TryGetValue(domain.Id, out var list);

                var dto = dataSetSummaryMapper.Export(domain);
                dto.MetaDates = domain.MetadataJsonFields
                    .Select(f => new AssignedMetaDateDTO
                    {
                        MetadataKey = f.MetadataKey,
                        MetadataId = f.FieldId,
                        CollectionSchemaVerified = list?.Contains(f.MetadataKey) ?? false
                    })
                    .ToList();
                return dto;
            }).ToList();
        }

        return dtos;
    }
}
