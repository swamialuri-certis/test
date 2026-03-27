namespace SqsProcessor.Services;

public interface IMessageProcessorService
{
    Task ProcessAsync(string messageId, string messageBody, CancellationToken cancellationToken = default);
}
