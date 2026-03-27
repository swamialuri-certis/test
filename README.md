# SqsProcessor

A production-ready .NET 8 AWS Lambda function that processes messages from an Amazon SQS queue and forwards them to two downstream HTTP APIs (API A and API B).

## Architecture Overview

```
Amazon SQS Queue
      │
      ▼
┌─────────────────────┐
│   SqsEventHandler   │  ← Lambda entry point
└────────┬────────────┘
         │
         ▼
┌─────────────────────────┐
│  MessageProcessorService│
│  ┌─────────────────┐    │
│  │ Idempotency     │    │  ← Deduplication via in-memory store
│  ├─────────────────┤    │
│  │ MessageValidator│    │  ← JSON schema + field validation
│  ├─────────────────┤    │
│  │ MessageTransfor-│    │  ← Deserialize & normalize
│  │ mer             │    │
│  ├─────────────────┤    │
│  │ MessageMapper   │    │  ← Shape for each downstream API
│  └─────────────────┘    │
└──────────┬──────────────┘
           │
     ┌─────┴──────┐
     ▼            ▼
 API Client A  API Client B
 (POST /api/   (POST /api/
  orders)       events)
```

## Message Processing Flow

1. **Receive** — Lambda is triggered by SQS; each record is processed individually.
2. **Idempotency** — The `messageId` is checked against an in-memory store to skip duplicates.
3. **Validation** — Required JSON fields are verified; invalid messages are logged and dropped (not retried).
4. **Transformation** — Raw JSON is deserialized into `IncomingMessage` and normalized into a `TransformedMessage` (currency uppercased, correlation ID attached).
5. **Mapping** — `TransformedMessage` is shaped into `ApiARequest` and `ApiBRequest`.
6. **Dispatch** — Both requests are sent concurrently via typed `HttpClient` instances.
7. **Mark Processed** — On success the `messageId` is stored in the idempotency cache.

## Project Structure

```
SqsProcessor/
├── Handlers/         # Lambda entry point
├── Services/         # Core business logic
├── Clients/          # Typed HTTP clients
├── Models/           # Request / response DTOs
├── Validation/       # Input validation
├── Configuration/    # Strongly-typed settings
└── Utilities/        # Retry & error helpers
```

## Configuration

Edit `appsettings.json` or override via environment variables:

| Key | Description |
|-----|-------------|
| `Sqs__QueueName` | SQS queue name |
| `Api__EndpointA` | Base URL for API A |
| `Api__EndpointB` | Base URL for API B |
| `Api__TimeoutSeconds` | HTTP timeout (default 30) |
| `Retry__MaxRetries` | Max HTTP retries (default 3) |
| `Retry__BaseDelaySeconds` | Exponential back-off base (default 2) |
| `Aws__Region` | AWS region (default `us-east-1`) |
| `Aws__SecretsManagerSecretName` | Secrets Manager secret path |

## Error Handling Strategy

- **Validation errors** — logged as warnings; message is silently dropped (no SQS retry).
- **HTTP errors** — retried with exponential back-off via Polly.
- **Unhandled exceptions** — logged as errors and re-thrown so SQS can retry or send to the DLQ.
- Per-message failures do not abort the entire batch; processing continues for remaining records.

## Retry Strategy

Polly `WaitAndRetryAsync` is configured on both `HttpClient` instances:

- Handles `HttpRequestException` and `TaskCanceledException`.
- Delay = `BaseDelaySeconds ^ attempt` (exponential back-off).
- Default: 3 retries, 2-second base → delays of 2 s, 4 s, 8 s.

## Running Locally

```bash
dotnet restore
dotnet build
dotnet publish -c Release -o publish/
```

To invoke locally with the Lambda Test Tool:

```bash
dotnet tool install -g Amazon.Lambda.TestTool-8.0
dotnet lambda-test-tool-8.0
```

## Deployment

```bash
dotnet tool install -g Amazon.Lambda.Tools
dotnet lambda deploy-function SqsProcessor
```

Lambda handler: `SqsProcessor::SqsProcessor.Handlers.SqsEventHandler::HandleAsync`
