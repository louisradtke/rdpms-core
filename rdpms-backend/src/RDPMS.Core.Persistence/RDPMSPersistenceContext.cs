using Microsoft.EntityFrameworkCore;
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

        WriteInfo();
    }

    public RDPMSPersistenceContext(DatabaseConfiguration dbConfiguration, LaunchConfiguration launchConfiguration)
    {
        DbConfiguration = dbConfiguration;
        _dbInitMode = launchConfiguration.InitDatabase;
        
        WriteInfo();
    }

    private void WriteInfo()
    {
        Console.WriteLine($"Using {DbConfiguration.GetConnectionDescription()}");
        Console.WriteLine($"Database init mode: {_dbInitMode}");
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

        optionsBuilder
            .UseSeeding(SeedData);
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
        // create global project
        if (context.Set<Project>().Find(RDPMSConstants.GlobalProjectId) is null)
        {
            context.Set<Project>().Add(new Project("_global")
            {
                Id = RDPMSConstants.GlobalProjectId,
                Description = "The instances global mockup project."
            });
        }

        if (_dbInitMode == LaunchConfiguration.DatabaseInitMode.Development)
        {
            if (context.Set<DataStore>().Find(RDPMSConstants.DummyS3StoreId) is not DataStore store)
            {
                context.Set<DataStore>().Add(new S3DataStore("dummy-s3-store")
                {
                    Id = RDPMSConstants.DummyS3StoreId,
                    Bucket = "dummy-bucket",
                    EndpointUrl = "http://localhost:9000",
                    KeyPrefix = "data/",
                    AccessKeyReference = "direct://dummy-access-key",
                    SecretKeyReference = "direct://dummy-secret-key"
                });
            }
            else if (store is not S3DataStore)
                throw new IllegalArgumentException("Dummy S3 store is not of type S3DataStore");
        }

        foreach (var contentType in DefaultValues.Types)
        {
            if (context.Set<ContentType>().Find(contentType.Id) is null)
            {
                context.Set<ContentType>().Add(contentType);
            }
        }
    }
}