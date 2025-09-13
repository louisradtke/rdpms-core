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
    public DatabaseConfiguration DbConfiguration { get; }
    private readonly LaunchConfiguration.DatabaseInitMode _dbInitMode;
    private readonly ILogger<RDPMSPersistenceContext>? _logger;

    public DbSet<ContentType> Types { get; set; }
    public DbSet<DataFile> DataFiles { get; set; }
    public DbSet<FileStorageReference> FileStorageReferences { get; set; }
    public DbSet<DataSet> DataSets { get; set; }
    public DbSet<DataStore> DataStores { get; set; }
    public DbSet<DataCollectionEntity> DataCollections { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<PipelineInstance> PipelineInstances { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DataSetUsedForJobsRelation> DataSetsUsedForJobs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<LabelSharingPolicy> LabelSharingPolicies { get; set; }

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
        
        var stack = new System.Diagnostics.StackTrace();
        logT($"PID={Environment.ProcessId}, TID={Environment.CurrentManagedThreadId}, Call stack:\n{stack}\n" +
             $"----------------------------------------");
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
            .UseSeeding(SeedData);
            // .UseAsyncSeeding((ctx, b, t) => Task.Run(() => SeedData(ctx, b), t));
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // set up data file
        model.Entity<DataFile>()
            .HasMany<FileStorageReference>(e => e.Locations);
        model.Entity<DataFile>()
            .Ignore(e => e.IsTimeSeries) // computed
            .Ignore(e => e.IsDeleted);

        model.Entity<DataSet>()
            .HasOne(e => e.CreateJob);

        // set up (dual) many-to-many mapping of PipelineInstance, because DataSet holds SourceForJobs and CreateJob
        model.Entity<DataSetUsedForJobsRelation>()
            .HasKey(e => new {e.JobId, e.SourceDatasetId});
        model.Entity<Job>()
            .HasMany(e => e.SourceDatasets)
            .WithMany(e => e.SourceForJobs)
            .UsingEntity(nameof(DataSetUsedForJobsRelation));
        model.Entity<Job>()
            .HasMany(e => e.OutputDatasets)
            .WithOne(e => e.CreateJob);
        
        // set up many-to-many mapping of Label and DataSet
        model.Entity<LabelsAssignedToDataSetsRelation>()
            .HasKey(e => new {e.LabelId, e.DataSetId});
        model.Entity<DataSet>()
            .HasMany(e => e.AssignedLabels)
            .WithMany(e => e.AssignedToDataSets)
            .UsingEntity<LabelsAssignedToDataSetsRelation>();

        // set up data store hierarchy
        model.Entity<DataStore>()
            .HasDiscriminator(e => e.StorageType)
            .HasValue<S3DataStore>(StorageType.S3); // static is skipped intentionally

        // set up file storage hierarchy
        model.Entity<FileStorageReference>()
            .HasDiscriminator(e => e.StorageType)
            .HasValue<S3FileStorageReference>(StorageType.S3)
            .HasValue<StaticFileStorageReference>(StorageType.Static);
        
        // set up many-to-many mapping of Label and Project
        model.Entity<Project>()
            .HasMany<Label>(e => e.Labels)
            .WithOne(e => e.ParentProject);
        model.Entity<Project>()
            .HasMany<LabelSharingPolicy>(e => e.SharedLabels);
        model.Entity<Project>()
            .HasMany<ContentType>(e => e.AllFileTypes);
        model.Entity<Project>()
            .HasData(new Project("_global")
            {
                Id = RDPMSConstants.GlobalProjectId,
                Description = "The instances global mockup project."
            });

        // set up fast searching for Job ID and State
        model.Entity<Job>()
            .HasIndex(e => e.LocalId);
        model.Entity<Job>()
            .HasIndex(e => e.State);

        model.Entity<PipelineInstance>()
            .HasIndex(e => e.LocalId);
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