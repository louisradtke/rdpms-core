using RDPMS.Core.Persistence;

namespace RDPMS.Core.Main.Configuration.Database;

public class SqliteDatabaseConfiguration : DatabaseConfigurationBase
{
    public SqliteDatabaseConfiguration()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var folderPath = Environment.GetFolderPath(folder);
        Path = System.IO.Path.Join(folderPath, "rdpms-debug.db");
    }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string? Path { get; set; }

    public override string GetConnectionString()
    {
        return $"Data Source={Path}";
    }

    public override DatabaseProvider Provider => DatabaseProvider.Sqlite;
}