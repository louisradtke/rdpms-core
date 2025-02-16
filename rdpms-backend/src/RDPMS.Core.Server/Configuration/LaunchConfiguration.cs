using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Configuration.Database;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RDPMS.Core.Server.Configuration;

/// <summary>
/// Settings for the application that won't change during runtime
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class LaunchConfiguration
{
    // ReSharper disable once UnusedMember.Global
    public DatabaseConfigurationBase? DatabaseConfiguration { get; set; }
    public string ListeningUrl { get; set; } = "http://localhost:5000";

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

    public void CopyToRuntimeConfiguration(RuntimeConfiguration runtimeConfiguration)
    {
        // nothing to do, yet
    }
}