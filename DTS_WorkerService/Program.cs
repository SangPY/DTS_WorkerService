using DTS_WorkerService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DTS Worker Service";
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
