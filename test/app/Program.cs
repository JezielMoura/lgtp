using System.Text.Json.Serialization;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using Npgsql;
using OpenTelemetry.Resources;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options => 
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

ConfigureOpenTelemetry(builder.Services, builder.Logging);

var app = builder.Build();

app.MapGet("/", async () => {
    var httpClient = new HttpClient();
    var result = await httpClient.GetStringAsync("https://www.google.com.br");
    return Results.Ok(result);
});

app.MapGet("/e", () => {
    throw new Exception("Error");
});

app.Run();

static void ConfigureOpenTelemetry(IServiceCollection services, ILoggingBuilder logging)
{
    services.AddSingleton(TracerProvider.Default.GetTracer("DemoApp"));

    var builder = services.AddOpenTelemetry();

    builder.ConfigureResource(resource => resource.AddService("DemoApp"));
    
    builder.WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(["Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel"])
        .AddOtlpExporter());

    builder.WithTracing(tracing => tracing
        .AddSource("DemoApp")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql()
        .AddOtlpExporter());

    logging.AddOpenTelemetry(options => {
        options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("DemoApp"));
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;
        options.AddOtlpExporter();
    });
}

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(string))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{

}
