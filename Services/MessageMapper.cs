using SqsProcessor.Models;

namespace SqsProcessor.Services;

public class MessageMapper : IMessageMapper
{
    public ApiARequest MapToApiARequest(TransformedMessage message)
    {
        return new ApiARequest
        {
            UserId = message.UserId,
            OrderId = message.OrderId,
            Amount = message.Amount,
            Currency = message.Currency,
            CorrelationId = message.CorrelationId,
            ProcessedAt = DateTimeOffset.UtcNow
        };
    }

    public ApiBRequest MapToApiBRequest(TransformedMessage message)
    {
        return new ApiBRequest
        {
            ReferenceId = message.OrderId,
            UserId = message.UserId,
            TotalAmount = message.Amount,
            CurrencyCode = message.Currency,
            CorrelationId = message.CorrelationId,
            EventTimestamp = message.Timestamp,
            Metadata = message.Metadata
        };
    }
}
