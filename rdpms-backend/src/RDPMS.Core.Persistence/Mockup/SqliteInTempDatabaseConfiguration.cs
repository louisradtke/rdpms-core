using RDPMS.Core.Infra.Configuration.Database;

namespace RDPMS.Core.Persistence.Mockup;

public class SqliteInTempDatabaseConfiguration : DatabaseConfiguration
{
    public override string GetConnectionString()
    {
        var filePath = GetFilePath();
        return $"Data Source={filePath}";
    }

    private static string GetFilePath()
    {
        var filePath = Path.Join(
            Path.GetTempPath(),
            "rdpms",
            "dummy.db"
        );
        return filePath;
    }

    public override string GetConnectionDescription() =>
        $"SQLite file at '{GetFilePath()}'";
    
    public override DatabaseProvider Provider => DatabaseProvider.Sqlite;
}