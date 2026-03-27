using System.Text.Json;
using Serilog;

namespace SqsProcessor.Validation;

public class MessageValidator : IMessageValidator
{
    private static readonly string[] RequiredFields = ["userId", "orderId", "amount", "currency", "timestamp"];

    public bool Validate(string messageBody, out string validationError)
    {
        validationError = string.Empty;

        if (string.IsNullOrWhiteSpace(messageBody))
        {
            validationError = "Message body is null or empty.";
            Log.Warning("Validation failed: {ValidationError}", validationError);
            return false;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(messageBody);
        }
        catch (JsonException ex)
        {
            validationError = $"Message body is not valid JSON: {ex.Message}";
            Log.Warning("Validation failed: {ValidationError}", validationError);
            return false;
        }

        using (document)
        {
            var root = document.RootElement;

            foreach (var field in RequiredFields)
            {
                if (!root.TryGetProperty(field, out _))
                {
                    validationError = $"Required field '{field}' is missing.";
                    Log.Warning("Validation failed: {ValidationError}", validationError);
                    return false;
                }
            }

            if (root.TryGetProperty("amount", out var amountElement))
            {
                if (!amountElement.TryGetDecimal(out var amount) || amount < 0)
                {
                    validationError = "Field 'amount' must be a non-negative numeric value.";
                    Log.Warning("Validation failed: {ValidationError}", validationError);
                    return false;
                }
            }

            if (root.TryGetProperty("timestamp", out var timestampElement))
            {
                if (!DateTimeOffset.TryParse(timestampElement.GetString(), out _))
                {
                    validationError = "Field 'timestamp' must be a valid ISO 8601 date-time string.";
                    Log.Warning("Validation failed: {ValidationError}", validationError);
                    return false;
                }
            }
        }

        return true;
    }
}
