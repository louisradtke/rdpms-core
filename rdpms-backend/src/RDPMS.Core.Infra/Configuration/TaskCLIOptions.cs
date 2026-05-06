using CommandLine;

namespace RDPMS.Core.Infra.Configuration;

[Verb("task", HelpText = "Run a named maintenance task.")]
public class TaskCLIOptions
{
    public const string RefreshProjectionsTask = "refresh_projections";
    public static readonly IReadOnlyCollection<string> RegisteredTasks =
    [
        RefreshProjectionsTask
    ];

    [Value(0, Required = false, MetaName = "task", HelpText = "Task name to run. Omit to list registered tasks.")]
    public string? TaskName { get; set; }

    [Option('c', "config", Required = false, HelpText = "Set path to configuration file.")]
    public string? ConfigurationFilePath { get; set; }

    [Option(longName: "init-db", Required = false, HelpText = "Initialize database before running the task.")]
    public string? InitDatabase { get; set; } = null;

    public void CopyToLaunchConfiguration(LaunchConfiguration launchConfiguration)
    {
        if (InitDatabase is "prod" or "production")
            launchConfiguration.InitDatabase = LaunchConfiguration.DatabaseInitMode.Production;
        else if (InitDatabase is "dev" or "development")
            launchConfiguration.InitDatabase = LaunchConfiguration.DatabaseInitMode.Development;
    }

    public bool Validate(out string? reason)
    {
        if (InitDatabase is not null &&
            InitDatabase is not ("prod" or "production" or "dev" or "development"))
        {
            reason = "Invalid value for --init-db, allowed are \"production\" or \"development\"";
            return false;
        }

        if (TaskName is not null && !RegisteredTasks.Contains(TaskName))
        {
            reason = $"Unknown task '{TaskName}'. Supported tasks: {string.Join(", ", RegisteredTasks)}";
            return false;
        }

        reason = null;
        return true;
    }
}
