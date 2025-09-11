namespace RDPMS.Core.Infra.Configuration.Database;

public class PostgresDatabaseConfiguration : DatabaseConfiguration
{
    public override string GetConnectionString() =>
        $"Host={Host};Port={Port};Database={Database};Username={User};Password={Password}";

    public override string GetConnectionDescription() =>
        $"Postgres Server at {Host}:{Port}, Database: {Database}";

    public override DatabaseProvider Provider => DatabaseProvider.Postgres;
    
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Database { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}