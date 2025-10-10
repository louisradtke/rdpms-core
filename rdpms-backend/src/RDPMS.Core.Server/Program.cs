using System.Reflection;
using System.Text.RegularExpressions;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Config;
using NLog.Extensions.Logging;
using RDPMS.Core.Infra;
using RDPMS.Core.Infra.AppInitialization;
using RDPMS.Core.Infra.Configuration;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services;

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


        // add services to the collection.
        builder.Services.AddSingleton(runtimeConfig);
        builder.Services.AddSingleton(launchConfig);
        builder.Services.AddSingleton(launchConfig.DatabaseConfiguration);

        builder.Services.AddScoped<DbContext, RDPMSPersistenceContext>();

        // file mapper interface, for now
        builder.Services.AddAttributedServices([typeof(Program).Assembly]);
        builder.Services.AddSingleton<ContentTypeDTOMapper>();
        builder.Services.AddSingleton<DataCollectionSummaryDTOMapper>();
        builder.Services.AddSingleton<DataSetSummaryDTOMapper>();
        builder.Services.AddSingleton<FileCreateRequestDTOMapper>();
        builder.Services.AddSingleton<FileCreateResponseDTOMapper>();
        builder.Services.AddSingleton<FileSummaryDTOMapper>();
        builder.Services.AddSingleton<StoreSummaryDTOMapper>();

        builder.Services.AddScoped<IDataSetRepository, DataSetRepository>();
        builder.Services.AddScoped<IDataStoreRepository, DataStoreRepository>();
        builder.Services.AddScoped<IDataFileRepository, DataFileRepository>();
        builder.Services.AddScoped<IContentTypeRepository, ContentTypeRepository>();
        builder.Services.AddScoped<IDataCollectionRepository, DataCollectionRepository>();
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ISlugRepository, SlugRepository>();

        builder.Services.AddScoped<IDataSetService, DataSetService>();
        builder.Services.AddScoped<IStoreService,StoreService>();
        builder.Services.AddScoped<IFileService, DataFileService>();
        builder.Services.AddScoped<IContentTypeService, ContentTypeService>();
        builder.Services.AddScoped<IDataCollectionEntityService, DataCollectionEntityService>();
        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<ISlugService, SlugService>();


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

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(options =>
        {
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
                var ctx = app.Services.GetService<DbContext>()!;
                await ctx.Database.EnsureCreatedAsync();
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
                await ctx.Database.EnsureCreatedAsync();

                // let's see what happens if we try to migrate the database :)
                // await ctx.Database.MigrateAsync();
                // await ctx.SaveChangesAsync();

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
        }
    }

    public static async Task Main(string[] args)
    {
        Task? appTask = null;
        CommandLine.Parser.Default.ParseArguments<SeedingCLIOptions, ServerCLIOptions>(args)
            .WithParsed<SeedingCLIOptions>(opts => appTask = RunSeeding(opts))
            .WithParsed<ServerCLIOptions>(opts => appTask = RunServer(opts))
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