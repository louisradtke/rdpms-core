namespace RDPMS.Core.Persistence;

public abstract class DatabaseConfiguration
{
    public abstract string GetConnectionString();
    public abstract DatabaseProvider Provider { get; }

    // ReSharper disable once UnusedMember.Global
    public string Type { get; set; } = string.Empty;
}
