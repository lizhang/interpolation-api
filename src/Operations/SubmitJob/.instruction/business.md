# SubmitJob — Business Logic

## Endpoint

`POST /api/jobs`

## Step Sequence

1. Validate request
2. Resolve upload session from DynamoDB via `IDynamoDbService`
3. Generate `jobId`
4. Write job record to DynamoDB via `IDynamoDbService`
5. Publish job message to SQS via `ISqsService`
6. Increment submissions counter via `IDynamoDbService.IncrementSubmissionAsync` (fire-and-forget on failure — log warning and continue)
7. Return response DTO

## Validation Rules

- `email` — required, non-empty
- `uploadId` — required, non-empty

## ID Generation

```
jobId = "job_" + Guid.NewGuid().ToString("N")
```

## Upload Session Resolution

Retrieve the upload session using `email` + `uploadId`. If not found, throw `InvalidOperationException` — the controller returns 404.

## DynamoDB Job Record

Table keys and attributes:

| Field | Value |
|---|---|
| `email` (PK) | from request |
| `jobId` (SK) | generated |
| `startFrameKey` | from upload session |
| `endFrameKey` | from upload session |
| `createdAt` | UTC timestamp |
| `resultsJson` | `""` (empty string) |

## SQS Message

Publish a JSON message containing:

| Field | Value |
|---|---|
| `jobId` | generated |
| `email` | from request |
| `uploadId` | from request |
| `startFrameKey` | from upload session |
| `endFrameKey` | from upload session |
| `createdAt` | same UTC timestamp written to DynamoDB |

## Response Shape

See `jobs.response.json`. Return `jobId` and `status: "QUEUED"`.
