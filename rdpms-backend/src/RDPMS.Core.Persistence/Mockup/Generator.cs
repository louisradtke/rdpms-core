using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence.Mockup;

public static class Generator
{
    public static void FillDatabase()
    {
        var ctx = new RDPMSPersistenceContext(new TestDatabaseConnection());
        FillDatabase(ctx);
    }
    
    public static void FillDatabase(RDPMSPersistenceContext ctx)
    {
        ctx.Database.EnsureCreated();
        
        var store = new DataStoreEntity("DefaultStore");
        var containter = new DataContainerEntity("DefaultContainer")
        {
            DefaultDataStore = store
        };

        var type = new ContentTypeEntity()
        {
            Abbreviation = "txt"
        };

        var dataFile = new DataFileEntity("test.txt")
        {
            FileType = type,
        };

        var existingDataset = new DataSetEntity("OriginDataset")
        {
            AncestorDatasetIds = [],
            CreateJob = null,
            Files = [dataFile]
        };

        var logEntry = new LogSectionEntity()
        {
            LogContent = "Stuff made here."
        };

        var job1 = new JobEntityEntity("BuildDataset")
        {
            OutputContainer = containter,
            SourceDatasets = [existingDataset],
            Logs = [logEntry]
        };

        var pipeline = new PipelineInstanceEntity()
        {
            Jobs = [job1]
        };
        
        ctx.PipelineInstances.Add(pipeline);
        ctx.SaveChanges();
    }

    class TestDatabaseConnection : DatabaseConfigurationBase
    {
        public override string GetConnectionString()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var folderPath = Environment.GetFolderPath(folder);
            var path = Path.Join(folderPath, "rdpms-debug.db");
            return $"Data Source={path}";
        }
        
        public override DatabaseProvider Provider => DatabaseProvider.Sqlite;
    }
}