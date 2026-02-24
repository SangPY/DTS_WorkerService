using DTS_WorkerService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DTS Worker Service";
});

// 👉 Setup Serilog from appsettings
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
//host.Run();
try
{
    Log.Information("=== SERVICE STARTING ===");
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service crashed");
}
finally
{
    Log.CloseAndFlush();
}
