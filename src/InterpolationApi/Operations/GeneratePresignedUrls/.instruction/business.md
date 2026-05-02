# GeneratePresignedUrls — Business Logic

## Endpoint

`POST /api/uploads`

## Step Sequence

1. Validate request
2. Generate `uploadId`
3. Derive S3 keys for both files
4. Generate presigned POST uploads via `IS3Service`
5. Persist upload session via `IDynamoDbService`
6. Return response DTO

## Validation Rules

- `email` — required, non-empty
- `startFile.name`, `startFile.contentType`, `startFile.size` — all required; size must be > 0
- `endFile.name`, `endFile.contentType`, `endFile.size` — all required; size must be > 0

## ID Generation

```
uploadId = "upl_" + Guid.NewGuid().ToString("N")
```

## S3 Key Format

```
uploads/{uploadId}/start.png
uploads/{uploadId}/end.png
```

Keys are fixed — always `start.png` and `end.png` regardless of the original file name.

## Presigned POST

- Expiry: 15 minutes
- Generated via `IS3Service.GeneratePresignedPostAsync(key, contentType)`

## Upload Session (persisted to DynamoDB)

Fields stored so `/api/jobs` can resolve the session later:

| Field | Value |
|---|---|
| `email` | from request |
| `uploadId` | generated |
| `startFrameKey` | S3 key for start frame |
| `endFrameKey` | S3 key for end frame |
| `createdAt` | UTC timestamp |

## Response Shape

See `uploads.response.json`. Return `uploadId`, and for each frame: the S3 `key` plus the presigned POST `url` and `fields`.
