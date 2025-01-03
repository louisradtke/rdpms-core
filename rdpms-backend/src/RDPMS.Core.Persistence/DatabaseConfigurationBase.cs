namespace RDPMS.Core.Persistence;

public abstract class DatabaseConfigurationBase
{
    public abstract string GetConnectionString();
    public abstract DatabaseProvider Provider { get; }
}
