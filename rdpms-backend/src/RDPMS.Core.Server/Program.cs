using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CommandLine;
using Microsoft.OpenApi.Models;
using RDPMS.Core.Server.Configuration;

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

builder.WebHost
    .UseUrls(launchConfig.ListeningUrl);

// Add services to the container.
builder.Services.AddSingleton(runtimeConfig);
builder.Services.AddSingleton(launchConfig);

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
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
}

app.UseAuthorization();

app.MapControllers();

app.Run();