using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Configuration.Database;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RDPMS.Core.Server.Configuration;

/// <summary>
/// Settings for the application that won't change during runtime
/// </summary>
public class LaunchConfiguration
{
    public DatabaseConfigurationBase DatabaseConfiguration { get; set; } = null!;
    public string ListeningUrl { get; set; } = "http://localhost:5000";

    public void CopyFromCLIOptions(CLIOptions options)
    {
        ListeningUrl = string.IsNullOrEmpty(options.ListeningUrl)? ListeningUrl : options.ListeningUrl;
    }

    public static LaunchConfiguration LoadParamsFromYaml(string yamlFilePath)
    {
        var yaml = File.ReadAllText(yamlFilePath);
        
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .WithTypeDiscriminatingNodeDeserializer(o =>
            {
                var valueMappings = new Dictionary<string, Type>
                {
                    { "postgres", typeof(PostgresDatabaseConfiguration) },
                    { "sqlite", typeof(SqliteDatabaseConfiguration) }
                };
                o.AddKeyValueTypeDiscriminator<DatabaseConfigurationBase>("type",
                    valueMappings);
            })
            .Build();
        
        var launchConfig = deserializer.Deserialize<LaunchConfiguration>(yaml);
        return launchConfig;
    }
}