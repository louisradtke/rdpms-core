using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RDPMS.Core.Infra;
using RDPMS.Core.Infra.Configuration;
using RDPMS.Core.Infra.Configuration.Database;
using RDPMS.Core.Infra.Exceptions;
using RDPMS.Core.Persistence.Mockup;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public class RDPMSPersistenceContext : DbContext
{
    private DatabaseConfiguration DbConfiguration { get; }
    private readonly LaunchConfiguration.DatabaseInitMode _dbInitMode;
    private readonly ILogger<RDPMSPersistenceContext>? _logger;

    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    public DbSet<ContentType> Types { get; private set; }
    public DbSet<DataFile> DataFiles { get; private set; }
    public DbSet<FileStorageReference> FileStorageReferences { get; private set; }
    public DbSet<DataSet> DataSets { get; private set; }
    public DbSet<DataStore> DataStores { get; private set; }
    public DbSet<DataCollectionEntity> DataCollections { get; private set; }
    public DbSet<Job> Jobs { get; private set; }
    public DbSet<PipelineInstance> PipelineInstances { get; private set; }
    public DbSet<Tag> Tags { get; private set; }
    public DbSet<Project> Projects { get; private set; }
    public DbSet<LabelSharingPolicy> LabelSharingPolicies { get; private set; }
    public DbSet<Slug> Slugs { get; private set; }
    public DbSet<MetadataJsonField> MetadataJsonFields { get; private set; }
    public DbSet<JsonSchemaEntity> JsonSchemas { get; private set; }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Local

    // EF Core requires a public constructor with no parameters. Tho, the DI framework automatically resolves the
    // constructor with the most resolvable parameters.
    // see https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#multiple-constructor-discovery-rules
    public RDPMSPersistenceContext()
    {
        DbConfiguration = new SqliteInTempDatabaseConfiguration();
        _dbInitMode = LaunchConfiguration.DatabaseInitMode.None;

        LogInit();
    }

    public RDPMSPersistenceContext(
        ILogger<RDPMSPersistenceContext> logger,
        DatabaseConfiguration dbConfiguration,
        LaunchConfiguration launchConfiguration)
    {
        _logger = logger;
        DbConfiguration = dbConfiguration;
        _dbInitMode = launchConfiguration.InitDatabase;
        
        LogInit();
    }

    private void LogInit()
    {
        var logD = new Action<string>(Console.WriteLine);
        if (_logger is not null)
            logD = s => _logger.LogDebug(s);

        var logT = new Action<string>(Console.WriteLine);
        if (_logger is not null)
            logT = s => _logger.LogTrace(s);

        logD($"Using {DbConfiguration.GetConnectionDescription()}");
        logD($"Database init mode: {_dbInitMode}");
        
        // var stack = new System.Diagnostics.StackTrace();
        // logT($"PID={Environment.ProcessId}, TID={Environment.CurrentManagedThreadId}, Call stack:\n{stack}\n" +
        //      $"----------------------------------------");
        logT($"PID={Environment.ProcessId}, TID={Environment.CurrentManagedThreadId}");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        switch (DbConfiguration.Provider)
        {
            case DatabaseProvider.Sqlite:
                optionsBuilder = optionsBuilder.UseSqlite(DbConfiguration.GetConnectionString());
                break;
            case DatabaseProvider.Postgres:
                optionsBuilder = optionsBuilder.UseNpgsql(DbConfiguration.GetConnectionString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // change behavior from fail-fast to log warning
        // see https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#new-behavior
        if (_dbInitMode == LaunchConfiguration.DatabaseInitMode.Development)
        {
            optionsBuilder = optionsBuilder.ConfigureWarnings(b =>
                b.Log(RelationalEventId.PendingModelChangesWarning));
        }

        optionsBuilder
            .UseSeeding(SeedData)
            .UseAsyncSeeding(SeedDataAsync);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // set up data file
        model.Entity<DataFile>()
            .HasMany<FileStorageReference>(e => e.Locations)
            .WithOne()
            .HasForeignKey(r => r.FileFid);
        model.Entity<DataFile>()
            .Ignore(e => e.IsTimeSeries) // computed
            .Ignore(e => e.IsDeleted);

        model.Entity<DataSet>()
            .HasOne(e => e.CreateJob);
        model.Entity<DataSet>()
            .HasMany<DataFile>(ds => ds.Files)
            .WithOne()
            .HasForeignKey(f => f.ParentId);

        model.Entity<DataCollectionEntity>()
            .HasMany<DataSet>(c => c.ContainedDatasets)
            .WithOne()
            .HasForeignKey(ds => ds.ParentId);

        // set up (dual) many-to-many mapping of PipelineInstance, because DataSet holds SourceForJobs and CreateJob
        model.Entity<Job>()
            .HasMany(e => e.SourceDatasets)
            .WithMany(e => e.SourceForJobs);
        
        // set up job output data sets, or tell data set in which job it was created
        model.Entity<Job>()
            .HasMany(e => e.OutputDatasets)
            .WithOne(e => e.CreateJob);

        // set up data store hierarchy
        model.Entity<DataStore>()
            .HasDiscriminator(e => e.StorageType)
            .HasValue<S3DataStore>(StorageType.S3); // static is skipped intentionally
        model.Entity<DataStore>()
            .HasMany<FileStorageReference>()
            .WithOne()
            .HasForeignKey(r => r.StoreFid);

        // set up file storage hierarchy
        model.Entity<FileStorageReference>()
            .HasDiscriminator(e => e.StorageType)
            .HasValue<S3FileStorageReference>(StorageType.S3)
            .HasValue<StaticFileStorageReference>(StorageType.Static)
            .HasValue<DbFileStorageReference>(StorageType.Db);

        // set up many-to-many mapping of Label and Project
        model.Entity<Project>()
            .HasMany<Label>(e => e.Labels)
            .WithOne(e => e.ParentProject);
        model.Entity<Project>()
            .HasMany<LabelSharingPolicy>(e => e.SharedLabels);
        model.Entity<Project>()
            .HasMany<ContentType>(e => e.FileTypes);
        model.Entity<Project>()
            .HasData(new Project("_global")
            {
                Id = RDPMSConstants.GlobalProjectId,
                Slug = "_global",
                Description = "The instances global mockup project."
            });
        model.Entity<Project>()
            .HasMany<DataCollectionEntity>(p => p.DataCollections)
            .WithOne()
            .HasForeignKey(c => c.ParentId);
        model.Entity<Project>()
            .HasMany<DataStore>(p => p.DataStores)
            .WithOne()
            .HasForeignKey(s => s.ParentId);
        model.Entity<Project>()
            .HasMany<Tag>(p => p.Tags)
            .WithOne(t => t.ParentProject);

        model.Entity<DataSet>()
            .HasMany(ds => ds.AssignedTags)
            .WithMany(t => t.AssignedToDataSets);

        // set up fast searching for Job ID and State
        model.Entity<Job>()
            .HasIndex(e => e.LocalId);
        model.Entity<Job>()
            .HasIndex(e => e.State);

        model.Entity<PipelineInstance>()
            .HasIndex(e => e.LocalId);

        // many-to-many of DataFile to MetadataJsonField
        model.Entity<MetadataJsonField>()
            .HasOne(f => f.Value)
            .WithMany()
            .IsRequired();

        // many-to-many of MetadataJsonField to JsonSchemaEntity
        model.Entity<MetadataJsonField>()
            .HasMany(f => f.ValidatedSchemas)
            .WithMany();

        // set-up and many-to-many of DataEntityMetadataJsonField MetadataJsonField
        model.Entity<DataEntityMetadataJsonField>()
            .HasKey(link => link.Id);
        model.Entity<DataEntityMetadataJsonField>()
            .HasIndex(link => link.MetadataJsonFieldId);
        model.Entity<DataEntityMetadataJsonField>()
            .HasIndex(link => link.DataFileId);
        model.Entity<DataEntityMetadataJsonField>()
            .HasIndex(link => link.DataSetId);
        model.Entity<DataEntityMetadataJsonField>()
            .HasOne(link => link.MetadataJsonField)
            .WithMany()
            .HasForeignKey(link => link.MetadataJsonFieldId)
            .OnDelete(DeleteBehavior.Cascade);

        // metadata fields assigned to data sets
        model.Entity<DataEntityMetadataJsonField>()
            .HasOne(link => link.DataSet)
            .WithMany(ds => ds.MetadataJsonFields)
            .HasForeignKey(link => link.DataSetId)
            .OnDelete(DeleteBehavior.Cascade);
        model.Entity<DataEntityMetadataJsonField>()
            .HasIndex(link => new { link.DataSetId, link.MetadataKey });

        // metadata fields assigned to data files
        model.Entity<DataEntityMetadataJsonField>()
            .HasOne(link => link.DataFile)
            .WithMany(df => df.MetadataJsonFields)
            .HasForeignKey(link => link.DataFileId)
            .OnDelete(DeleteBehavior.Cascade);
        model.Entity<DataEntityMetadataJsonField>()
            .HasIndex(link => new { link.DataFileId, link.MetadataKey });
    }

    private async Task SeedDataAsync(DbContext context, bool _, CancellationToken token)
    {
        foreach (var contentType in DefaultValues.DefaultTypes)
        {
            if (await context.Set<ContentType>().FindAsync(contentType.Id, token) is null)
            {
                await context.Set<ContentType>().AddAsync(contentType, token);
            }
        }

        if (_dbInitMode != LaunchConfiguration.DatabaseInitMode.Development) return;

        if (await context.Set<DataStore>()
                .FindAsync(RDPMSConstants.DummyS3StoreId, token) is not { } store)
        {
            await context.Set<DataStore>().AddAsync(DefaultValues.DummyS3Store, token);
        }
        else if (store is not S3DataStore)
            throw new IllegalArgumentException("Dummy S3 store is not of type S3DataStore");

        await SaveChangesAsync(token);
        
        if (await context.Set<DataCollectionEntity>()
                .FindAsync(RDPMSConstants.DummyDataCollectionId, token) is null)
        {
            await context.Set<DataCollectionEntity>()
                .AddAsync(await DefaultValues.GetDummyDataCollectionAsync(context, token), token);
        }
        
        await SaveChangesAsync(token);
    }

    private void SeedData(DbContext context, bool _)
    {
        foreach (var contentType in DefaultValues.DefaultTypes)
        {
            if (context.Set<ContentType>().Find(contentType.Id) is null)
            {
                context.Set<ContentType>().Add(contentType);
            }
        }

        if (_dbInitMode != LaunchConfiguration.DatabaseInitMode.Development) return;

        if (context.Set<DataStore>().Find(RDPMSConstants.DummyS3StoreId) is not { } store)
        {
            context.Set<DataStore>().Add(DefaultValues.DummyS3Store);
        }
        else if (store is not S3DataStore)
            throw new IllegalArgumentException("Dummy S3 store is not of type S3DataStore");

        SaveChanges();
        
        if (context.Set<DataCollectionEntity>().Find(RDPMSConstants.DummyDataCollectionId) is null)
        {
            context.Set<DataCollectionEntity>().Add(DefaultValues.GetDummyDataCollection(context));
        }
        
        SaveChanges();
    }
}
