using CommandLine;
using RDPMS.Core.Server.Configuration;

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
launchConfig.CopyFromCLIOptions(cliOptions);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .UseUrls(launchConfig.ListeningUrl);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();