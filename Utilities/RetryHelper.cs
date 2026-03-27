using Polly;
using Polly.Retry;
using Serilog;

namespace SqsProcessor.Utilities;

public static class RetryHelper
{
    public static AsyncRetryPolicy CreateHttpRetryPolicy(int maxRetries, int baseDelaySeconds)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: maxRetries,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(baseDelaySeconds, attempt)),
                onRetry: (exception, timeSpan, attempt, context) =>
                {
                    Log.Warning(exception,
                        "Retry {Attempt}/{MaxRetries} after {Delay}s due to: {Message}",
                        attempt, maxRetries, timeSpan.TotalSeconds, exception.Message);
                });
    }
}
