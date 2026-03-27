using SqsProcessor.Models;

namespace SqsProcessor.Services;

public interface IMessageMapper
{
    ApiARequest MapToApiARequest(TransformedMessage message);
    ApiBRequest MapToApiBRequest(TransformedMessage message);
}
