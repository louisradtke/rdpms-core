using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.OpenApi.Models;
using RDPMS.Core.Persistence;
using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Configuration;
using RDPMS.Core.Server.Model.Mappers;
using RDPMS.Core.Server.Model.Repositories;
using RDPMS.Core.Server.Model.Repositories.Infra;
using RDPMS.Core.Server.Services;

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
    .WithParsed(opts => cliOptions = opts);

var launchConfig =
    LaunchConfiguration.LoadParamsFromYaml(cliOptions.ConfigurationFilePath ?? "debug.yaml");
cliOptions.CopyToLaunchConfiguration(launchConfig);

var runtimeConfig = new RuntimeConfiguration();
launchConfig.CopyToRuntimeConfiguration(runtimeConfig);

// build the app
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        name: "ConfigCorsPolicy",
        policy => policy.WithOrigins(launchConfig.AllowedOrigins.ToArray()));
});

builder.WebHost
    .UseUrls(launchConfig.ListeningUrl);

ArgumentNullException.ThrowIfNull(launchConfig.DatabaseConfiguration);

// Add services to the container.
builder.Services.AddSingleton(runtimeConfig);
builder.Services.AddSingleton(launchConfig);
builder.Services.AddSingleton(launchConfig.DatabaseConfiguration);

builder.Services.AddSingleton<RDPMSPersistenceContext>();

builder.Services.AddSingleton<ContainerSummaryDTOMapper>();
builder.Services.AddSingleton<StoreSummaryDTOMapper>();

builder.Services.AddSingleton<IGenericRepository<DataSet>, DataSetRepository>();
builder.Services.AddSingleton<IGenericRepository<DataStore>, DataStoreRepository>();
builder.Services.AddSingleton<DataFileRepository>();
builder.Services.AddSingleton<ContentTypeRepository>();
builder.Services.AddSingleton<DataContainerRepository>();

builder.Services.AddSingleton<IDataSetService, DataSetService>();
builder.Services.AddSingleton<IStoreService,StoreService>();
builder.Services.AddSingleton<IFileService, FileService>();
builder.Services.AddSingleton<IContentTypeService, ContentTypeService>();
builder.Services.AddSingleton<IContainerService, ContainerService>();

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
            Title = $"My API {description}",
            Version = description,
            Description = $"API Version {description}"
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
            $"API {description.ApiVersion}");
    }
});

app.UseCors("ConfigCorsPolicy");

app.UseAuthorization();

app.MapControllers();

// TODO make somehow nicer
app.Services.GetService<RDPMSPersistenceContext>()!.Database.EnsureCreated();

app.Run();