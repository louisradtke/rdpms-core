using RDPMS.Core.Persistence;

namespace RDPMS.Core.Main.Configuration;

public class SqliteDatabaseConfiguration : DatabaseConfigurationBase
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