using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Serilog;
using SqsProcessor.Clients;
using SqsProcessor.Configuration;
using SqsProcessor.Services;
using SqsProcessor.Validation;

namespace SqsProcessor;

public static class Startup
{
    public static IServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        var appSettings = new AppSettings();
        configuration.Bind(appSettings);

        ConfigureLogging();

        var services = new ServiceCollection();
        services.AddSingleton(appSettings);
        services.AddSingleton<IMessageValidator, MessageValidator>();
        services.AddSingleton<IMessageTransformer, MessageTransformer>();
        services.AddSingleton<IMessageMapper, MessageMapper>();
        services.AddSingleton<IdempotencyService>();
        services.AddScoped<IMessageProcessorService, MessageProcessorService>();

        var retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: appSettings.Retry.MaxRetries,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromSeconds(Math.Pow(appSettings.Retry.BaseDelaySeconds, attempt)),
                onRetry: (outcome, timeSpan, attempt, context) =>
                {
                    Log.Warning(outcome.Exception,
                        "HTTP retry {Attempt} after {Delay}s: {Message}",
                        attempt, timeSpan.TotalSeconds, outcome.Exception?.Message ?? "non-success status");
                });

        services.AddHttpClient<IApiClientA, ApiClientA>(client =>
        {
            client.BaseAddress = new Uri(appSettings.Api.EndpointA);
            client.Timeout = TimeSpan.FromSeconds(appSettings.Api.TimeoutSeconds);
        })
        .AddPolicyHandler(retryPolicy);

        services.AddHttpClient<IApiClientB, ApiClientB>(client =>
        {
            client.BaseAddress = new Uri(appSettings.Api.EndpointB);
            client.Timeout = TimeSpan.FromSeconds(appSettings.Api.TimeoutSeconds);
        })
        .AddPolicyHandler(retryPolicy);

        return services.BuildServiceProvider();
    }

    private static void ConfigureLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
