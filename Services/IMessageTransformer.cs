using SqsProcessor.Models;

namespace SqsProcessor.Services;

public interface IMessageTransformer
{
    TransformedMessage Transform(string messageBody, string correlationId);
}
