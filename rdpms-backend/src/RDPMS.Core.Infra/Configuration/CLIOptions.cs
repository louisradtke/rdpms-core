using CommandLine;

namespace RDPMS.Core.Infra.Configuration;

public class CLIOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
    
    [Option('u', "url", Required = false, HelpText = "Set URL this application is listening on.")]
    public string? ListeningUrl { get; set; }
    
    [Option('c', "config", Required = false, HelpText = "Set path to configuration file.")]
    public string? ConfigurationFilePath { get; set; }
    
    [Option(longName: "init-db", Required = false, HelpText = "Initialize database on application startup.")]
    public string? InitDatabase { get; set; } = null;

    public void CopyToLaunchConfiguration(LaunchConfiguration launchConfiguration)
    {
        launchConfiguration.ListeningUrl =
            string.IsNullOrEmpty(ListeningUrl) ? launchConfiguration.ListeningUrl : ListeningUrl;
        if (InitDatabase is "prod" or "production")
            launchConfiguration.InitDatabase = LaunchConfiguration.DatabaseInitMode.Production;
        else if (InitDatabase is "dev" or "development")
            launchConfiguration.InitDatabase = LaunchConfiguration.DatabaseInitMode.Development;
    }
    
    public static bool Validate(CLIOptions options, out string? reason)
    {
        if (options.InitDatabase is not null &&
            options.InitDatabase is not ("prod" or "production" or "dev" or "development"))
        {
            reason = "Invalid value for --init-db, allowed are \"production\" or \"development\"";
            return false;
        }

        reason = null;
        return true;
    }
}
