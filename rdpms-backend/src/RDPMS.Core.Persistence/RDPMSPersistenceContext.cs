using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public class RDPMSPersistenceContext : DbContext
{
    public DatabaseConfigurationBase Configuration { get; }
    public DbSet<ContentTypeEntity> Types { get; set; }
    public DbSet<DataFileEntity> DataFiles { get; set; }
    public DbSet<DataSetEntity> DataSets { get; set; }
    public DbSet<DataStoreEntity> DataStores { get; set; }
    public DbSet<DataContainerEntity> DataContainers { get; set; }
    public DbSet<JobEntityEntity> Jobs { get; set; }
    public DbSet<PipelineInstanceEntity> PipelineInstances { get; set; }
    public DbSet<TagEntity> Tags { get; set; }
    public DbSet<DataSetUsedForJobsEntity> DataSetsUsedForJobs { get; set; }

    public RDPMSPersistenceContext(DatabaseConfigurationBase configuration)
    {
        Configuration = configuration;
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

        model.Entity<DataSetEntity>()
            .HasOne(e => e.CreateJob);

        // set up many-to-many mapping of PipelineInstance
        model.Entity<DataSetUsedForJobsEntity>()
            .HasKey(e => new {e.JobId, e.SourceDatasetId});
        model.Entity<JobEntityEntity>()
            .HasMany(e => e.SourceDatasets)
            .WithMany(e => e.SourceForJobs)
            .UsingEntity(nameof(DataSetUsedForJobsEntity));
        model.Entity<JobEntityEntity>()
            .HasMany(e => e.OutputDatasets)
            .WithOne(e => e.CreateJob);

        // set up fast searching for Job ID and State
        model.Entity<JobEntityEntity>()
            .HasAlternateKey(e => e.LocalId);
        model.Entity<JobEntityEntity>()
            .HasAlternateKey(e => e.State);

        model.Entity<PipelineInstanceEntity>()
            .HasAlternateKey(e => e.LocalId);
    }
}
