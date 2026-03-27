using System.Net.Http.Json;
using Serilog;
using SqsProcessor.Models;

namespace SqsProcessor.Clients;

public class ApiClientB : IApiClientB
{
    private readonly HttpClient _httpClient;

    public ApiClientB(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendAsync(ApiBRequest request, CancellationToken cancellationToken = default)
    {
        Log.Debug("Sending request to API B for ReferenceId {ReferenceId}.", request.ReferenceId);

        var response = await _httpClient.PostAsJsonAsync("/api/events", request, cancellationToken);

        Log.Information("API B responded with status {StatusCode} for ReferenceId {ReferenceId} CorrelationId {CorrelationId}.",
            (int)response.StatusCode, request.ReferenceId, request.CorrelationId);

        response.EnsureSuccessStatusCode();
    }
}
