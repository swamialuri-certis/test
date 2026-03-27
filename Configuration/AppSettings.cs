namespace SqsProcessor.Configuration;

public class AppSettings
{
    public SqsSettings Sqs { get; set; } = new();
    public ApiSettings Api { get; set; } = new();
    public RetrySettings Retry { get; set; } = new();
    public AwsSettings Aws { get; set; } = new();
}

public class SqsSettings
{
    public string QueueName { get; set; } = string.Empty;
}

public class ApiSettings
{
    public string EndpointA { get; set; } = string.Empty;
    public string EndpointB { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}

public class RetrySettings
{
    public int MaxRetries { get; set; } = 3;
    public int BaseDelaySeconds { get; set; } = 2;
}

public class AwsSettings
{
    public string Region { get; set; } = "us-east-1";
    public string SecretsManagerSecretName { get; set; } = string.Empty;
}
