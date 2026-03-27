using System.Collections.Concurrent;
using Serilog;

namespace SqsProcessor.Services;

public class IdempotencyService
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _processedMessages = new();

    public bool HasBeenProcessed(string messageId)
    {
        return _processedMessages.ContainsKey(messageId);
    }

    public void MarkAsProcessed(string messageId)
    {
        _processedMessages[messageId] = DateTimeOffset.UtcNow;
        Log.Debug("Message {MessageId} marked as processed for idempotency.", messageId);
    }
}
