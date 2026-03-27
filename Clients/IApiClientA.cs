using SqsProcessor.Models;

namespace SqsProcessor.Clients;

public interface IApiClientA
{
    Task SendAsync(ApiARequest request, CancellationToken cancellationToken = default);
}
