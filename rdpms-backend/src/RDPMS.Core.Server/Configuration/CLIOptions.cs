using CommandLine;

namespace RDPMS.Core.Server.Configuration;

public class CLIOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }
    
    [Option('u', "url", Required = false, HelpText = "Set URL this application is listening on.")]
    public string? ListeningUrl { get; set; }
    
    [Option('c', "config", Required = false, HelpText = "Set path to configuration file.")]
    public string? ConfigurationFilePath { get; set; }
}
