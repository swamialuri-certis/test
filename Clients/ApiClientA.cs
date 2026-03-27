using System.Net.Http.Json;
using Serilog;
using SqsProcessor.Models;

namespace SqsProcessor.Clients;

public class ApiClientA : IApiClientA
{
    private readonly HttpClient _httpClient;

    public ApiClientA(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendAsync(ApiARequest request, CancellationToken cancellationToken = default)
    {
        Log.Debug("Sending request to API A for OrderId {OrderId}.", request.OrderId);

        var response = await _httpClient.PostAsJsonAsync("/api/orders", request, cancellationToken);

        Log.Information("API A responded with status {StatusCode} for OrderId {OrderId} CorrelationId {CorrelationId}.",
            (int)response.StatusCode, request.OrderId, request.CorrelationId);

        response.EnsureSuccessStatusCode();
    }
}
