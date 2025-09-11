namespace RDPMS.Core.Infra.Configuration.Database;

public abstract class DatabaseConfiguration
{
    public abstract string GetConnectionString();
    
    /// <summary>
    /// This text can appear in logs and give users debug information about the DB config/connection.
    /// Sensitive information may not be exopsed.
    /// </summary>
    /// <returns>A string describing the db connection.</returns>
    public abstract string GetConnectionDescription();
    public abstract DatabaseProvider Provider { get; }

    // ReSharper disable once UnusedMember.Global
    public string Type { get; set; } = string.Empty;
}
