using Serilog;
using SqsProcessor.Clients;
using SqsProcessor.Validation;

namespace SqsProcessor.Services;

public class MessageProcessorService : IMessageProcessorService
{
    private readonly IMessageValidator _validator;
    private readonly IMessageTransformer _transformer;
    private readonly IMessageMapper _mapper;
    private readonly IApiClientA _apiClientA;
    private readonly IApiClientB _apiClientB;
    private readonly IdempotencyService _idempotencyService;

    public MessageProcessorService(
        IMessageValidator validator,
        IMessageTransformer transformer,
        IMessageMapper mapper,
        IApiClientA apiClientA,
        IApiClientB apiClientB,
        IdempotencyService idempotencyService)
    {
        _validator = validator;
        _transformer = transformer;
        _mapper = mapper;
        _apiClientA = apiClientA;
        _apiClientB = apiClientB;
        _idempotencyService = idempotencyService;
    }

    public async Task ProcessAsync(string messageId, string messageBody, CancellationToken cancellationToken = default)
    {
        var startTime = DateTimeOffset.UtcNow;
        var correlationId = Guid.NewGuid().ToString();

        Log.Information("Processing message {MessageId} with CorrelationId {CorrelationId}.", messageId, correlationId);

        if (_idempotencyService.HasBeenProcessed(messageId))
        {
            Log.Information("Message {MessageId} already processed. Skipping.", messageId);
            return;
        }

        if (!_validator.Validate(messageBody, out var validationError))
        {
            Log.Warning("Validation failed for message {MessageId}: {ValidationError}", messageId, validationError);
            return;
        }

        try
        {
            var transformedMessage = _transformer.Transform(messageBody, correlationId);

            var apiARequest = _mapper.MapToApiARequest(transformedMessage);
            var apiBRequest = _mapper.MapToApiBRequest(transformedMessage);

            // Dispatch to both APIs concurrently (they are independent)
            Log.Information("Sending payloads to API A and API B for user {UserId} with CorrelationId {CorrelationId}.", transformedMessage.UserId, correlationId);
            await Task.WhenAll(
                _apiClientA.SendAsync(apiARequest, cancellationToken),
                _apiClientB.SendAsync(apiBRequest, cancellationToken));

            _idempotencyService.MarkAsProcessed(messageId);

            var duration = DateTimeOffset.UtcNow - startTime;
            Log.Information("Message {MessageId} processed successfully in {DurationMs}ms.", messageId, duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing message {MessageId} with CorrelationId {CorrelationId}.", messageId, correlationId);
            throw;
        }
    }
}
