# CLAUDE.md

## Project Overview

Backend for a **frame interpolation web application**. Users upload two key frames; the system generates in-between frames using an AI model.

The backend:
- generates S3 presigned upload URLs
- accepts job submissions
- writes job metadata to DynamoDB
- queues processing jobs in SQS

---

## Tech Stack

| Concern | Technology |
|---|---|
| Runtime | .NET 10 |
| Hosting | AWS Lambda (HttpApi event source) |
| Object storage | Amazon S3 |
| Queue | Amazon SQS |
| Database | Amazon DynamoDB |
| HTTP framework | ASP.NET Core (`MapControllers`) |
| OpenAPI | Microsoft.AspNetCore.OpenApi + Scalar |
| DI / config | Built-in ASP.NET Core |

---

## Repository Structure

See [`backend-file-map.md`](.ai/context/backend-file-map.md) — read it before writing or refactoring any code.

### Pre-Search Protocol

Before searching broadly (Glob, Grep, or any directory listing):

1. Read `.ai/context/backend-file-map.md`
2. Read `.ai/context/file-dependencies.md`
3. Then inspect only the files that are likely relevant based on those maps
4. If the reference is outdated, trust actual code over the reference

---

## Endpoints

The system exposes exactly **four endpoints**. Do not add others.

| # | Method | Path | Handler |
|---|---|---|---|
| 0 | GET | `/api/health` | `Program.cs` minimal endpoint — no controller, no operation |
| 1 | POST | `/api/uploads` | `JobsController` → `GeneratePresignedUrlsOperation` |
| 2 | POST | `/api/jobs` | `JobsController` → `SubmitJobOperation` |
| 3 | GET | `/api/stats` | `StatsController` → `GetStatsOperation` |

Business logic for each endpoint lives in its operation's `.instruction/business.md`.

---

## Architecture

Follows the `backend-architecture` skill. For adding new endpoints, follow the `operation-from-instructions` skill.

---

## Logging

Log in operations using structured logging (`_logger.LogInformation("... {Field}", value)`):
- Operation start with identifying fields (e.g. `Email`, `UploadId`)
- Key business events (e.g. session created, job queued)
- Downstream failures as `Warning` or `Error` before re-throwing

Never log:
- Presigned POST fields or URLs
- Secrets or tokens
- Full request/response payloads

---

## Validation

Validate in the operation before calling any service. Throw `ArgumentException` — the controller maps it to 400.

Fields always required:
- `Email` — must be non-empty
- `UploadId` — must be non-empty when present
- File metadata: `ContentType` non-empty, `Size > 0`

---

## Configuration

Two config files for two enviroment: `appsettings.local.json` and `appsettings.production.json`. Do not add others.

Bound to `AppSettings`:
- `App:S3BucketName`
- `App:SqsQueueUrl`
- `App:DynamoDbTableName`
- `App:AwsRegion`

---

## Hard constraints

- Do not add endpoints beyond the four listed above
- Contracts live in each operation's `.instruction/` folder — no top-level `/api-contracts/`
- Use strongly typed configuration via `AppSettings` — no `IConfiguration` directly in operations
