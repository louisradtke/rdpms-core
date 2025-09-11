namespace RDPMS.Core.Infra.Configuration.Database;

public class SqliteDatabaseConfiguration : DatabaseConfiguration
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

    public override string GetConnectionDescription() =>
        $"SQLite file at '{Path}'";

    public override DatabaseProvider Provider => DatabaseProvider.Sqlite;
}