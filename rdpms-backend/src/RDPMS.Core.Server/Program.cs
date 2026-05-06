using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using RDPMS.Core.Infra;
using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Infra.Configuration;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.MetadataProjection;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.DTO.V1;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services;
using RDPMS.Core.Server.Services.MetadataProjection;
using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    private static async Task RunServer(ServerCLIOptions serverCLIOptions)
    {
        if (!serverCLIOptions.Validate(out var err))
        {
            await Console.Error.WriteLineAsync(err);
            Environment.Exit(1);
        }

        var launchConfig =
            LaunchConfiguration.LoadParamsFromYaml(serverCLIOptions.ConfigurationFilePath ?? "debug.yaml");
        serverCLIOptions.CopyToLaunchConfiguration(launchConfig);

        var runtimeConfig = new RuntimeConfiguration();
        launchConfig.CopyToRuntimeConfiguration(runtimeConfig);


        // init logging
        NLog.LogManager.Setup().LoadConfiguration(builder => ConfigureLogging(launchConfig, builder));

        
        // build the app
        var builder = WebApplication.CreateBuilder();

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();

        builder.Services.AddCors(options =>
        {
            // cors policy for every route
            options.AddPolicy(name: "DefaultCorsPolicy", policy => policy
                .WithOrigins(launchConfig.AllowedOrigins.ToArray() )
                .AllowAnyHeader()
                .AllowAnyMethod());

            // cors policy for file requests, manually set in controllers
            options.AddPolicy("ExternalCorsPolicy", policy =>
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        builder.WebHost
            .UseUrls(launchConfig.ListeningUrl);
        Console.WriteLine($"Listening on {launchConfig.ListeningUrl}");

        ArgumentNullException.ThrowIfNull(launchConfig.DatabaseConfiguration);


        AddCoreServices(builder.Services, launchConfig, runtimeConfig);


        // init api and api exploration
        var apiVersioningBuilder = builder.Services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);  // Default to v1
            options.ApiVersionReader = new UrlSegmentApiVersionReader(); // Uses /v{version} in the URL
        });

        apiVersioningBuilder.AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV"; // v1, v2, v3 format
            options.SubstituteApiVersionInUrl = true; // Replace {version} in URL with actual version
        });

        builder.Services.AddControllers(options =>
            {
                // Allow binding raw request bodies to byte[] for both json and octet-stream.
                options.InputFormatters.Insert(0, new RawBodyInputFormatter());
            })
            .AddJsonOptions(options => // show enum value in swagger.
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(options =>
        {
            options.MapType<JsonElement>(() => new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true
            });

            options.UseAllOfForInheritance();

            var apiVersions = new List<string> { "v1", "v2" };
            foreach (var description in apiVersions)
            {
                options.SwaggerDoc(description, new OpenApiInfo
                {
                    Title = $"RDPMS API {description}",
                    Version = description,
                    Description = $"RDPMS API {description}"
                });
            }

            // Add support for versioning via the URL
            // options.OperationFilter<SwaggerDefaultValues>();
    
            // add XML docstrings to Swagger docs
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
            
        
        // instantiate and launch app
        var app = builder.Build();

        // init swagger
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    $"API v{description.ApiVersion}");
            }
        });

        app.UseCors("DefaultCorsPolicy");
        app.UseAuthorization();
        app.MapControllers();

        // init db connection, run seeding, and launch app
        var logger = app.Services.GetService<ILogger<Program>>()!;
        try
        {
            if (launchConfig.InitDatabase is not LaunchConfiguration.DatabaseInitMode.None)
            {
                // create and seed database
                using var scope = app.Services.CreateScope();
                var ctx = scope.ServiceProvider.GetRequiredService<DbContext>();
                await ctx.Database.MigrateAsync();
                await ctx.SaveChangesAsync();
            }

            await app.RunAsync();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Unhandled exception. {EMessage}", e.Message);
        }
    }

    private static async Task RunSeeding(SeedingCLIOptions seedingCLIOptions)
    {
        if (!seedingCLIOptions.Validate(out var err))
        {
            await Console.Error.WriteLineAsync(err);
            Environment.Exit(1);
        }

        var launchConfig =
            LaunchConfiguration.LoadParamsFromYaml(seedingCLIOptions.ConfigurationFilePath ?? "debug.yaml");
        seedingCLIOptions.CopyToLaunchConfiguration(launchConfig);

        var runtimeConfig = new RuntimeConfiguration();
        launchConfig.CopyToRuntimeConfiguration(runtimeConfig);

        // init logging
        NLog.LogManager.Setup().LoadConfiguration(builder => ConfigureLogging(launchConfig, builder));
        var loggerFactory = new NLogLoggerFactory();
        var programLogger = loggerFactory.CreateLogger<Program>();
        var contextLogger = loggerFactory.CreateLogger<RDPMSPersistenceContext>();

        // init db connection, run seeding, and launch app
        try
        {
            if (launchConfig.InitDatabase is not LaunchConfiguration.DatabaseInitMode.None)
            {
                if (launchConfig.DatabaseConfiguration is null)
                {
                    programLogger.LogError("Database configuration is missing. Exiting.");
                    Environment.Exit(1);
                }

                // create and seed database
                // TODO: esp. seeding should not happen during normal application startup
                var ctx = new RDPMSPersistenceContext(contextLogger, launchConfig.DatabaseConfiguration, launchConfig);

                programLogger.LogInformation("Initializing database ...");
                // await ctx.Database.EnsureCreatedAsync();

                // let's see what happens if we try to migrate the database :)
                await ctx.Database.MigrateAsync();
                await ctx.SaveChangesAsync();

                var globalProject = await ctx.Projects.FindAsync(RDPMSConstants.GlobalProjectId);
                if (globalProject is null)
                {
                    programLogger.LogError("Probing for global project failed. Exiting.");
                    Environment.Exit(1);
                }
                programLogger.LogInformation("Database initialized.");
            }
        }
        catch (Exception e)
        {
            programLogger.LogCritical(e, "Unhandled exception. {EMessage}", e.Message);
            Environment.Exit(1);
        }
    }

    private static async Task RunTask(TaskCLIOptions taskCliOptions)
    {
        if (!taskCliOptions.Validate(out var err))
        {
            await Console.Error.WriteLineAsync(err);
            Environment.Exit(1);
        }

        if (taskCliOptions.TaskName is null)
        {
            Console.WriteLine("Registered tasks:");
            foreach (var taskName in TaskCLIOptions.RegisteredTasks)
            {
                Console.WriteLine($"- {taskName}");
            }

            return;
        }

        var launchConfig =
            LaunchConfiguration.LoadParamsFromYaml(taskCliOptions.ConfigurationFilePath ?? "debug.yaml");
        taskCliOptions.CopyToLaunchConfiguration(launchConfig);

        var runtimeConfig = new RuntimeConfiguration();
        launchConfig.CopyToRuntimeConfiguration(runtimeConfig);

        NLog.LogManager.Setup().LoadConfiguration(builder => ConfigureLogging(launchConfig, builder));

        var builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();
        AddCoreServices(builder.Services, launchConfig, runtimeConfig);

        var app = builder.Build();
        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        try
        {
            using var scope = app.Services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<DbContext>();
            if (launchConfig.InitDatabase is not LaunchConfiguration.DatabaseInitMode.None)
            {
                await ctx.Database.MigrateAsync();
                await ctx.SaveChangesAsync();
            }

            switch (taskCliOptions.TaskName)
            {
                case TaskCLIOptions.RefreshProjectionsTask:
                    await RefreshProjections(scope.ServiceProvider);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(taskCliOptions.TaskName),
                        $"Unknown task '{taskCliOptions.TaskName}'.");
            }
        }
        catch (Exception e)
        {
            logger.LogCritical(e, "Task {TaskName} failed. {EMessage}", taskCliOptions.TaskName, e.Message);
            Environment.Exit(1);
        }
    }

    private static async Task RefreshProjections(IServiceProvider serviceProvider)
    {
        var ctx = serviceProvider.GetRequiredService<DbContext>();
        var registry = serviceProvider.GetRequiredService<CachedMetadataProjectionRegistry>();
        var projectionService = serviceProvider.GetRequiredService<IEntityMetadataProjectionService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        var datasetSourceKeys = registry.GetAll()
            .Where(d => d.EntityType == typeof(DataSet))
            .Select(d => d.SourceKey)
            .Distinct()
            .ToList();

        var datasets = await ctx.Set<DataSet>().ToListAsync();
        var refreshed = 0;
        foreach (var dataset in datasets)
        {
            foreach (var sourceKey in datasetSourceKeys)
            {
                await projectionService.RefreshAsync(dataset, sourceKey);
                refreshed++;
            }
        }

        logger.LogInformation(
            "Refreshed {RefreshCount} metadata projection source(s) across {DatasetCount} dataset(s).",
            refreshed,
            datasets.Count);
    }

    private static void AddCoreServices(
        IServiceCollection services,
        LaunchConfiguration launchConfig,
        RuntimeConfiguration runtimeConfig)
    {
        services.AddSingleton(runtimeConfig);
        services.AddSingleton(launchConfig);
        ArgumentNullException.ThrowIfNull(launchConfig.DatabaseConfiguration);
        services.AddSingleton(launchConfig.DatabaseConfiguration);
        services.AddSingleton(new CachedMetadataProjectionRegistry()); // EF otherwise uses wrong ctor

        services.AddScoped<DbContext, RDPMSPersistenceContext>();

        // file mapper interface, for now
        services.AddAttributedServices([typeof(Program).Assembly]);
        services.AddSingleton<ContentTypeDTOMapper>();
        services.AddSingleton<DataCollectionSummaryDTOMapper>();
        services.AddSingleton<DataSetSummaryDTOMapper>();
        services.AddSingleton<FileCreateRequestDTOMapper>();
        services.AddSingleton<FileCreateResponseDTOMapper>();
        services.AddSingleton<FileSummaryDTOMapper>();
        services.AddSingleton<StoreSummaryDTOMapper>();

        services.AddScoped<IDataSetRepository, DataSetRepository>();
        services.AddScoped<IDataStoreRepository, DataStoreRepository>();
        services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
        services.AddScoped<IDataCollectionRepository, DataCollectionRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ISlugRepository, SlugRepository>();

        services.AddScoped<IDataSetService, DataSetService>();
        services.AddScoped<IStoreService,StoreService>();
        services.AddScoped<IFileService, DataFileService>();
        services.AddScoped<IContentTypeService, ContentTypeService>();
        services.AddScoped<IDataCollectionEntityService, DataCollectionEntityService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ISlugService, SlugService>();
        services.AddScoped<IS3Service, S3Service>();
        services.AddScoped<ISecretResolverService, SecretResolverService>();
        services.AddScoped<IMetadataService, MetadataService>();
        services.AddScoped<ISchemaService, SchemaService>();
        services.AddScoped<IMetadataDocumentReader, MetadataDocumentReader>();
        services.AddScoped<IEntityMetadataProjectionService, EntityMetadataProjectionService>();
    }

    public static async Task Main(string[] args)
    {
        Task? appTask = null;
        CommandLine.Parser.Default.ParseArguments<SeedingCLIOptions, ServerCLIOptions, TaskCLIOptions>(args)
            .WithParsed<SeedingCLIOptions>(opts => appTask = RunSeeding(opts))
            .WithParsed<ServerCLIOptions>(opts => appTask = RunServer(opts))
            .WithParsed<TaskCLIOptions>(opts => appTask = RunTask(opts))
            .WithNotParsed(errs =>
            {
                Console.Error.WriteLine("Failed to parse command line arguments");
                foreach (var err in errs)
                {
                    Console.Error.WriteLine(err.ToString());
                }
                Environment.Exit(1);
            });

        await (appTask ?? Task.CompletedTask);
    }

    private static void ConfigureLogging(LaunchConfiguration launchConfig, ISetupLoadConfigurationBuilder builder)
    {
        string? logFile = null;
        if (launchConfig.Logging.LogFileDir is not null)
        {
            logFile = Path.Join(launchConfig.Logging.LogFileDir,
                DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-rdpms-server.log");
        }

        var re = new Regex(@"Microsoft\.AspNetCore.*",
            RegexOptions.Compiled |
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);
        builder.ForLogger()
            .FilterMinLevel(launchConfig.Logging.ConsoleLogLevel)
            .FilterDynamicLog(e => !re.IsMatch(e.LoggerName))
            .WriteToConsole();
        if (logFile is null)
        {
            Console.WriteLine($"Logging to file is disabled");
            return;
        }
        builder.ForLogger()
            .FilterMinLevel(launchConfig.Logging.LogFileLevel)
            .WriteToFile(logFile);
        Console.WriteLine($"Logging to {logFile}");
    }
}
