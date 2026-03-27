using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SqsProcessor.Clients;
using SqsProcessor.Configuration;
using SqsProcessor.Services;
using SqsProcessor.Utilities;
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

        var retryPolicy = RetryHelper.CreateHttpRetryPolicy(
            appSettings.Retry.MaxRetries,
            appSettings.Retry.BaseDelaySeconds);

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
