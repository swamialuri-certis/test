using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SqsProcessor.Services;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace SqsProcessor.Handlers;

public class SqsEventHandler
{
    private readonly IServiceProvider _serviceProvider;

    public SqsEventHandler()
    {
        _serviceProvider = Startup.BuildServiceProvider();
    }

    public SqsEventHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task HandleAsync(SQSEvent sqsEvent, ILambdaContext context)
    {
        Log.Information("Lambda invoked with {RecordCount} SQS records.", sqsEvent.Records.Count);

        using var scope = _serviceProvider.CreateScope();
        var processor = scope.ServiceProvider.GetRequiredService<IMessageProcessorService>();

        foreach (var record in sqsEvent.Records)
        {
            var messageId = record.MessageId;
            var body = record.Body;

            Log.Information("Processing message {MessageId}.", messageId);

            try
            {
                await processor.ProcessAsync(messageId, body);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process message {MessageId}. Continuing with next message.", messageId);
            }
        }

        Log.Information("Lambda invocation complete.");
    }
}
