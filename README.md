Create a production-ready .NET 8 AWS Lambda project that processes AWS SQS events, transforms the message body, maps it into a target model, and sends the mapped payload to two external APIs.

The project must follow Clean Architecture, SOLID principles, and production-grade engineering standards.

1. Project Type

Create a .NET 8 AWS Lambda project that handles SQSEvent triggers using:

Amazon.Lambda.Core
Amazon.Lambda.SQSEvents
AWSSDK.SQS

The Lambda handler must process messages received from AWS SQS.

2. Architecture Requirements

Follow a clear layered architecture with separation of concerns.

Structure the project logically into these folders:

Handlers
Services
Clients
Models
Validation
Configuration
Utilities

Responsibilities:

Handlers
• Lambda entry point
• Receives SQSEvent
• Delegates processing to service layer

Services
• Message processing pipeline
• Transformation logic
• Mapping logic

Clients
• HTTP clients for external APIs

Models
• Incoming message model
• Transformed message model
• API request models

Validation
• Message validation logic

Configuration
• Application settings binding

Utilities
• Error handling helpers
• Retry helpers

Controllers or handlers must not contain business logic.

3. Message Processing Flow

The processing pipeline must follow this sequence:

SQS Event
→ Lambda Handler
→ Message Validation
→ Message Transformation
→ Model Mapping
→ Send to API A
→ Send to API B
→ Log result

Each message in the SQS batch must be processed individually.

4. Lambda Handler Requirements

The Lambda handler must:

Receive SQSEvent
Loop through Records
Extract MessageId and Body
Call MessageProcessorService.ProcessAsync()

The handler must remain thin and contain no business logic.

5. Message Validation

Validate incoming messages before processing.

Examples:

Message body must be valid JSON
Required fields must exist
Numeric values must be valid

If validation fails:

Log warning
Skip message processing

6. Transformation and Mapping

Implement a dedicated transformation service.

Steps:

Deserialize message body
Transform raw message into internal model
Map internal model into API request model

Mapping logic must be separated into a mapper class.

7. External API Communication

Send the mapped payload to two different APIs.

Requirements:

Use typed HttpClient via dependency injection
Send requests asynchronously
Handle HTTP response status codes properly

If API returns failure:

Log error
Retry according to retry policy

8. Retry Strategy

Implement retry logic using Polly.

Requirements:

Retry failed API calls
Maximum retries = configurable
Use exponential backoff

Retry must handle:

Network failures
HTTP timeouts
Temporary API errors

9. Error Handling

Implement robust error handling.

Handle these scenarios:

Invalid JSON message
Transformation failures
API failures
Timeouts
Secrets retrieval errors

Logging levels:

Information → message received
Warning → validation failure
Error → processing failure

10. Structured Logging

Use Serilog for structured logging.

Log these details:

MessageId
CorrelationId
Processing duration
API response status

Example structured logs:

Processing message {MessageId}

Sending payload to API A for user {UserId}

11. Configuration Management

Use appsettings.json for configuration.

Include settings for:

SQS queue name
API endpoint A
API endpoint B
Retry count
HTTP timeout
AWS region

Bind configuration using Microsoft.Extensions.Configuration.

12. Secrets Management

Retrieve sensitive values such as API credentials using AWS Secrets Manager.

Do not hardcode credentials.

13. Idempotency Handling

Implement basic idempotency protection.

Track processed MessageIds in memory cache.

If message already processed:

Skip processing.

14. Dependency Injection

Register all services using dependency injection.

Examples:

MessageProcessorService
ApiClientA
ApiClientB
MessageTransformer
MessageValidator

15. Asynchronous Programming

All external operations must use async/await.

Avoid blocking calls.

16. Documentation

Generate a README.md explaining:

Architecture overview
Message processing flow
Configuration setup
Error handling strategy
Retry strategy
How to run locally

17. Code Quality Requirements

Ensure the following:

SOLID principles followed
Separation of concerns maintained
No business logic inside Lambda handler
Proper exception handling
Clean folder structure

18. Output Requirements

Generate:

Complete folder structure
All required classes
Configuration files
Dependency injection setup
README.md documentation

Ensure the project compiles successfully and follows production-ready engineering practices.
