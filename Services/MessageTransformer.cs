using System.Text.Json;
using Serilog;
using SqsProcessor.Models;

namespace SqsProcessor.Services;

public class MessageTransformer : IMessageTransformer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public TransformedMessage Transform(string messageBody, string correlationId)
    {
        Log.Debug("Deserializing message body.");
        var incoming = JsonSerializer.Deserialize<IncomingMessage>(messageBody, JsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize message body.");

        return new TransformedMessage
        {
            UserId = incoming.UserId,
            OrderId = incoming.OrderId,
            Amount = incoming.Amount,
            Currency = incoming.Currency.ToUpperInvariant(),
            Timestamp = incoming.Timestamp,
            CorrelationId = correlationId,
            Metadata = incoming.Metadata
        };
    }
}
