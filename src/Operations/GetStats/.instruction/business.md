# GetStats — Business Logic

## Endpoint

`GET /api/stats`

## Step Sequence

1. Atomically increment the visit counter in DynamoDB
2. Return the current visit and submission totals

## DynamoDB Stats Record

Stats are stored in the existing table under a single record:

| Field | Value |
|---|---|
| `email` (PK) | `"app_stats"` |
| `jobId` (SK) | `"stats"` |
| `visits` (Number) | incremented on every GET /api/stats call |
| `submissions` (Number) | incremented when POST /api/jobs succeeds |

Use `UpdateItem` with `ADD visits :one` and `ReturnValues = ALL_NEW` to atomically increment and return the updated counts in one operation. DynamoDB creates the record and initializes missing counters to 0 automatically.

## Validation

None — no request body.

## Response Shape

See `stats.response.json`. Return `visits` and `submissions` as integers.
