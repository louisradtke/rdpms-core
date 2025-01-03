using RDPMS.Core.Persistence;

namespace RDPMS.Core.Main.Configuration;

/// <summary>
/// Settings for the application that won't change during runtime
/// </summary>
public class LaunchConfiguration
{
    DatabaseConfigurationBase DatabaseConfiguration { get; set; } = null!;
}