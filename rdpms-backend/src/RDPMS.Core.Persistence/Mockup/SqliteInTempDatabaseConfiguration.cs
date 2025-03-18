namespace RDPMS.Core.Persistence.Mockup;

public class SqliteInTempDatabaseConfiguration : DatabaseConfiguration
{
    public override string GetConnectionString()
    {
        var filePath = Path.Join(
            Path.GetTempPath(),
            "rdpms",
            "dummy.db"
        );
        return $"Data Source={filePath}";
    }

    public override DatabaseProvider Provider => DatabaseProvider.Sqlite;
}