using RDPMS.Core.Infra.Configuration.Database;
using RDPMS.Core.Infra.Util;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RDPMS.Core.Infra.Configuration;

/// <summary>
/// Settings for the application that won't change during runtime
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class LaunchConfiguration
{
    /// <summary>
    /// Access definitions for the main database.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public DatabaseConfiguration? DatabaseConfiguration { get; set; }
    
    /// <summary>
    /// Whether to init the database on application startup. This is not recommended for production.
    /// </summary>
    public DatabaseInitMode InitDatabase { get; set; } = DatabaseInitMode.None;
    
    /// <summary>
    /// Where to open the API endpoint.
    /// </summary>
    public string ListeningUrl { get; set; } = "http://localhost:5000";
    
    /// <summary>
    /// Origins which are allowed to access the API. Important for CORS.
    /// </summary>
    public List<string> AllowedOrigins { get; set; } = [];
    
    /// <summary>
    /// Whether the app is running in debug mode.
    /// </summary>
    public bool Debug { get; set; }

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
                o.AddKeyValueTypeDiscriminator<DatabaseConfiguration>("type",
                    valueMappings);
            })
            .Build();
        
        var launchConfig = deserializer.Deserialize<LaunchConfiguration>(yaml);
        
        if (launchConfig.DatabaseConfiguration is SqliteDatabaseConfiguration { Path: not null } sqliteCnf)
            sqliteCnf.Path = SubstituteVariablesHelper.SubstituteVariables(sqliteCnf.Path);

        return launchConfig;
    }

    public void CopyToRuntimeConfiguration(RuntimeConfiguration runtimeConfiguration)
    {
        // nothing to do, yet
    }
    
    public enum DatabaseInitMode
    {
        None,
        Production,
        Development
    }
}