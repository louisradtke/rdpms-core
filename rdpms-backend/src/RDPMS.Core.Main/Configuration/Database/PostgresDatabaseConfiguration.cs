using RDPMS.Core.Persistence;

namespace RDPMS.Core.Main.Configuration.Database;

public class PostgresDatabaseConfiguration : DatabaseConfigurationBase
{
    public override string GetConnectionString()
    {
        return $"Host={Host};Port={Port};Database={Database};Username={User};Password={Password}";
    }

    public override DatabaseProvider Provider => DatabaseProvider.Postgres;
    
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}