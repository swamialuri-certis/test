using SqsProcessor.Models;

namespace SqsProcessor.Clients;

public interface IApiClientB
{
    Task SendAsync(ApiBRequest request, CancellationToken cancellationToken = default);
}
