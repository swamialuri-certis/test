using System.Collections.Concurrent;
using Serilog;

namespace SqsProcessor.Services;

public class IdempotencyService
{
    // Lambda containers are reused across invocations; this cache persists for the lifetime of the container.
    // For distributed idempotency across multiple Lambda instances, use a persistent store (e.g., DynamoDB).
    private readonly ConcurrentDictionary<string, bool> _processedMessages = new();

    public bool HasBeenProcessed(string messageId)
    {
        return _processedMessages.ContainsKey(messageId);
    }

    public void MarkAsProcessed(string messageId)
    {
        _processedMessages[messageId] = true;
        Log.Debug("Message {MessageId} marked as processed for idempotency.", messageId);
    }
}
