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
        
        var store = new DataStore("DefaultStore");
        var containter = new DataContainer("DefaultContainer")
        {
            DefaultDataStore = store
        };

        var type = new ContentType()
        {
            Abbreviation = "txt"
        };

        var dataFile = new DataFile("test.txt")
        {
            FileType = type,
        };

        var existingDataset = new DataSet("OriginDataset")
        {
            AncestorDatasetIds = [],
            CreateJob = null,
            Files = [dataFile]
        };

        var logEntry = new LogSection()
        {
            LogContent = "Stuff made here."
        };

        var job1 = new Job("BuildDataset")
        {
            OutputContainer = containter,
            SourceDatasets = [existingDataset],
            Logs = [logEntry]
        };

        var pipeline = new PipelineInstance()
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