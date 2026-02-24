using DTS_WorkerService;
using Elasticsearch.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using System.Collections.Specialized;

var builder = Host.CreateApplicationBuilder(args);

// 👉 Load config
var elasticUri = builder.Configuration["Elastic:Uri"];
var apiKey = builder.Configuration["Elastic:ApiKey"];

Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("Service", "DTS_Worker")
    .Enrich.WithMachineName()
    .Enrich.FromLogContext()

    // 👉 Elastic Cloud sink
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticUri))
    {
        AutoRegisterTemplate = true,
        IndexFormat = "dts-worker-{0:yyyy.MM}",

        // ⭐ Elastic Cloud API key auth
        ModifyConnectionSettings = x =>
        {
            x.GlobalHeaders(new NameValueCollection
            {
                { "Authorization", $"ApiKey {apiKey}" }
            });
            return x;
        }
    })

    // 👉 fallback file log
    .WriteTo.File("C:\\Logs\\DTS_Worker\\log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger(); 

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DTS Worker Service";
});

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


public class ApiKeyHttpConnection : HttpConnection
{
    private readonly string _apiKey;
    public ApiKeyHttpConnection(string apiKey)
    {
        _apiKey = apiKey;
    }

    protected override HttpRequestMessage CreateHttpRequestMessage(RequestData requestData)
    {
        var message = base.CreateHttpRequestMessage(requestData);
        message.Headers.Add("Authorization", $"ApiKey {_apiKey}");
        return message;
    }
}
