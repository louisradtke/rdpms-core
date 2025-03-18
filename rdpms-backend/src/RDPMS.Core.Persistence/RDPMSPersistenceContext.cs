using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Mockup;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public class RDPMSPersistenceContext(DatabaseConfiguration configuration) : DbContext
{
    public DatabaseConfiguration Configuration { get; } = configuration;
    public DbSet<ContentType> Types { get; set; }
    public DbSet<DataFile> DataFiles { get; set; }
    public DbSet<DataSet> DataSets { get; set; }
    public DbSet<DataStore> DataStores { get; set; }
    public DbSet<DataContainer> DataContainers { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<PipelineInstance> PipelineInstances { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<DataSetUsedForJobs> DataSetsUsedForJobs { get; set; }

    // EF Core requires a public constructor with no parameters. Tho, the DI framework automatically resolves the
    // constructor with the most resolvable parameters.
    // see https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#multiple-constructor-discovery-rules
    public RDPMSPersistenceContext() : this(new SqliteInTempDatabaseConfiguration())
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        switch (Configuration.Provider)
        {
            case DatabaseProvider.Sqlite:
                options.UseSqlite(Configuration.GetConnectionString());
                break;
            case DatabaseProvider.Postgres:
                options.UseNpgsql(Configuration.GetConnectionString());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        
        // ignore computed properties
        model.Entity<DataFile>()
            .Ignore(e => e.IsTimeSeries);
        model.Entity<DataFile>()
            .Ignore(e => e.IsDeleted);

        model.Entity<DataSet>()
            .HasOne(e => e.CreateJob);

        // set up many-to-many mapping of PipelineInstance
        model.Entity<DataSetUsedForJobs>()
            .HasKey(e => new {e.JobId, e.SourceDatasetId});
        model.Entity<Job>()
            .HasMany(e => e.SourceDatasets)
            .WithMany(e => e.SourceForJobs)
            .UsingEntity(nameof(DataSetUsedForJobs));
        model.Entity<Job>()
            .HasMany(e => e.OutputDatasets)
            .WithOne(e => e.CreateJob);

        // set up fast searching for Job ID and State
        model.Entity<Job>()
            .HasAlternateKey(e => e.LocalId);
        model.Entity<Job>()
            .HasAlternateKey(e => e.State);

        model.Entity<PipelineInstance>()
            .HasAlternateKey(e => e.LocalId);
    }
}
