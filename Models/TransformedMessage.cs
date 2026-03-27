namespace SqsProcessor.Models;

public class TransformedMessage
{
    public string UserId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
