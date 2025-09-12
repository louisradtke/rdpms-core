using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Extensions.Logging;
using RDPMS.Core.Infra.Configuration;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Server;

// ReSharper disable once ClassNeverInstantiated.Global
internal class Program
{
    public static void Main(string[] args)
    {
        // handle config
        CLIOptions cliOptions = null!;
        CommandLine.Parser.Default.ParseArguments<CLIOptions>(args)
            .WithNotParsed(errs =>
            {
                Console.Error.WriteLine("Failed to parse command line arguments");
                foreach (var err in errs)
                {
                    Console.Error.WriteLine(err.ToString());
                }
                Environment.Exit(1);
            })
            .WithParsed(opts =>
            {
                if (!CLIOptions.Validate(opts, out var err))
                {
                    Console.Error.WriteLine(err);
                    Environment.Exit(1);
                }

                cliOptions = opts;
            });

        var launchConfig =
            LaunchConfiguration.LoadParamsFromYaml(cliOptions.ConfigurationFilePath ?? "debug.yaml");
        cliOptions.CopyToLaunchConfiguration(launchConfig);

        var runtimeConfig = new RuntimeConfiguration();
        launchConfig.CopyToRuntimeConfiguration(runtimeConfig);

        string? logFile = null;
        if (launchConfig.Logging.LogFileDir is not null)
        {
            logFile = Path.Join(launchConfig.Logging.LogFileDir,
                DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-rdpms-server.log");
        }

        // init logging
        NLog.LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger()
                .FilterMinLevel(launchConfig.Logging.ConsoleLogLevel)
                .WriteToConsole();
            if (logFile is not null)
            {
                builder.ForLogger()
                    .FilterMinLevel(launchConfig.Logging.LogFileLevel)
                    .WriteToFile(logFile);
                Console.WriteLine($"Logging to {logFile}");
            }
        });
        
        

        // build the app
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddNLog();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(
                name: "ConfigCorsPolicy",
                policy => policy.WithOrigins(launchConfig.AllowedOrigins.ToArray()));
        });

        builder.WebHost
            .UseUrls(launchConfig.ListeningUrl);
        Console.WriteLine($"Listening on {launchConfig.ListeningUrl}");

        ArgumentNullException.ThrowIfNull(launchConfig.DatabaseConfiguration);

        // Add services to the collection.
        builder.Services.AddSingleton(runtimeConfig);
        builder.Services.AddSingleton(launchConfig);
        builder.Services.AddSingleton(launchConfig.DatabaseConfiguration);

        builder.Services.AddSingleton<RDPMSPersistenceContext>();

        builder.Services.AddSingleton<ContentTypeDTOMapper>();
        builder.Services.AddSingleton<DataCollectionSummaryDTOMapper>();
        builder.Services.AddSingleton<DataSetSummaryDTOMapper>();
        builder.Services.AddSingleton<FileCreateRequestDTOMapper>();
        builder.Services.AddSingleton<FileCreateResponseDTOMapper>();
        builder.Services.AddSingleton<FileSummaryDTOMapper>();
        builder.Services.AddSingleton<StoreSummaryDTOMapper>();

        builder.Services.AddSingleton<IDataSetRepository, DataSetRepository>();
        builder.Services.AddSingleton<IDataStoreRepository, DataStoreRepository>();
        builder.Services.AddSingleton<IDataFileRepository, DataFileRepository>();
        builder.Services.AddSingleton<IContentTypeRepository, ContentTypeRepository>();
        builder.Services.AddSingleton<IDataCollectionRepository, DataCollectionRepository>();
        builder.Services.AddSingleton<IProjectRepository, ProjectRepository>();

        builder.Services.AddSingleton<IDataSetService, DataSetService>();
        builder.Services.AddSingleton<IStoreService,StoreService>();
        builder.Services.AddSingleton<IFileService, DataFileService>();
        builder.Services.AddSingleton<IContentTypeService, ContentTypeService>();
        builder.Services.AddSingleton<IDataCollectionEntityService, DataCollectionEntityService>();
        builder.Services.AddSingleton<IProjectService, ProjectService>();

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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // if (app.Environment.IsDevelopment())

        app.UseSwagger();
        // app.UseSwaggerUI();
        app.UseSwaggerUI(options =>
        {
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                    $"API v{description.ApiVersion}");
            }
        });

        app.UseCors("ConfigCorsPolicy");

        app.UseAuthorization();

        app.MapControllers();

        var logger = app.Services.GetService<ILogger<Program>>()!;
        try
        {
            if (launchConfig.InitDatabase is not LaunchConfiguration.DatabaseInitMode.None)
            {
                // create and seed database
                // TODO: esp. seeding should not happen during normal application startup
                var ctx = app.Services.GetService<RDPMSPersistenceContext>()!;
                ctx.Database.EnsureCreated();
                ctx.Database.Migrate();
                ctx.SaveChanges();
            }

            app.Run();
        }
        catch (Exception e)
        {
            logger.LogCritical(e, $"Unhandled exception. {e.Message}");
        }
        
    }
}