using Serilog;

namespace SqsProcessor.Utilities;

public static class ErrorHelper
{
    public static void LogAndSwallow(Exception exception, string context)
    {
        Log.Error(exception, "An error occurred in {Context}: {Message}", context, exception.Message);
    }

    public static void LogCritical(Exception exception, string context)
    {
        Log.Fatal(exception, "A critical error occurred in {Context}: {Message}", context, exception.Message);
    }
}
