using Polly;
using Polly.Retry;
using Serilog;

namespace SqsProcessor.Utilities;

public static class RetryHelper
{
    public static IAsyncPolicy<HttpResponseMessage> CreateHttpRetryPolicy(int maxRetries, int baseDelaySeconds)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt) * baseDelaySeconds),
                onRetry: (outcome, timeSpan, attempt, context) =>
                {
                    Log.Warning(outcome.Exception,
                        "Retry {Attempt}/{MaxRetries} after {Delay}s due to: {Message}",
                        attempt, maxRetries, timeSpan.TotalSeconds,
                        outcome.Exception?.Message ?? "non-success status");
                });
    }
}
