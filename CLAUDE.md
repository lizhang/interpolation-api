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

```
src/
├── Controllers/
│   ├── Dtos/           — HTTP request/response DTOs (one file per DTO)
│   └── JobsController.cs
├── Operations/
│   └── SubmitJob/      — example operation
│       ├── .instruction/
│       │   ├── business.md          ← authoritative spec
│       │   ├── jobs.request.json    ← API contract
│       │   └── jobs.response.json   ← API contract
│       ├── ISubmitJobOperation.cs
│       ├── SubmitJobOperation.cs
│       ├── SubmitJobInput.cs        ← domain input object
│       └── SubmitJobResult.cs       ← domain result object
├── Services/           — IS3Service.cs / S3Service.cs, etc.
├── Extensions/         — ServiceCollectionExtensions.cs
└── Program.cs
```

Contracts live inside each operation's `.instruction/` folder, not a top-level folder.

---

## Endpoints

The system exposes exactly **four endpoints**. Do not add others.

| # | Method | Path | Handler |
|---|---|---|---|
| 0 | GET | `/api/health` | `Program.cs` minimal endpoint — no controller, no operation |
| 1 | POST | `/api/uploads` | `JobsController` → `GeneratePresignedUrlsOperation` |
| 2 | POST | `/api/jobs` | `JobsController` → `SubmitJobOperation` |
| 3 | GET | `/api/stats` | `StatsController` → `GetStatsOperation` |

Business logic detail for endpoints 1–3 lives in the respective operation's `.instruction/business.md`.

---

## Architecture Layers

```
HTTP request
    └── Controller  (maps DTO → domain input, calls operation, maps result → DTO)
            └── Operation  (uses domain objects, no DTOs)
                    └── Service
                            └── AWS SDK
```

### Controller

- Lives in `src/Controllers/`
- Defines HTTP routing (`[Route]`), verb mapping (`[HttpPost]`), and response codes (`[ProducesResponseType]`)
- Binds the HTTP request to a DTO (`src/Controllers/Dtos/`), maps it to a domain input object, calls `operation.ExecuteAsync(...)`
- Maps the domain result back to a response DTO and returns it
- Translates exceptions to HTTP responses: `ArgumentException` → 400, `InvalidOperationException` → 404
- Contains **no business logic**

### Operation

- Lives in `src/Operations/<OperationName>/`
- Receives and returns domain objects (`<Name>Input`, `<Name>Result`) — never DTOs
- Validates the input (throw `ArgumentException` for invalid input)
- Coordinates business logic — calls services in the correct order
- Contains **no raw AWS SDK calls**

### Service

- Lives in `src/Services/`
- Wraps one AWS resource per service (`S3Service`, `SqsService`, `DynamoDbService`)
- Exposes **generic** interfaces — no domain-specific method names, no domain model types as parameters or returns
- `IDynamoDbService` uses `Dictionary<string, string>` for item attributes; counter operations use dedicated increment methods
- `ISqsService.SendMessageAsync<T>` accepts any serializable type — the operation decides the message shape
- Contains **no business logic**

---

## .instruction Folder Convention

Every operation folder **must** contain a `.instruction/` subfolder with:

| File | Purpose |
|---|---|
| `business.md` | Canonical source for business logic: key formats, DB schema, message shape, validation rules, step sequence |
| `<resource>.request.json` | Example JSON for the request body |
| `<resource>.response.json` | Example JSON for the response body |

When implementing or modifying an operation:

1. Read `.instruction/business.md` first — it is the authoritative spec.
2. Read the `.instruction/*.request.json` and `.instruction/*.response.json` contracts.
3. Generate request/response DTOs that exactly match those contracts.
4. Do not invent fields not in the contract.

---

## Adding a New Operation/Endpoint

Follow `.claude/skills/implement-endpoint/SKILL.md`.

Summary:
1. Create `src/Operations/<OperationName>/`
2. Create `.instruction/` inside it with `business.md` and contract JSON files
3. Create `IOperationNameOperation.cs`, `OperationNameOperation.cs`, `OperationNameRequest.cs`, `OperationNameResponse.cs`
4. Create a controller in `src/Controllers/` (or add a method to an existing one)
5. Register in `src/Extensions/ServiceCollectionExtensions.cs`

---

## Coding Rules

Use:
- `async` / `await` throughout
- Constructor dependency injection
- Explicit DTO models (no anonymous types in responses)
- Strongly typed configuration via `AppSettings`
- Structured logging (`_logger.LogInformation("... {Field}", value)`)
- `private` methods when an operation method body exceeds 200 lines

Avoid:
- Raw AWS SDK calls inside operations
- Business logic inside services
- Domain-specific method names on service interfaces (e.g. `SaveUploadSessionAsync`)
- Domain model types (`JobRecord`, etc.) as service interface parameters or return types
- Repository pattern, CQRS frameworks, over-engineering
- Creating a new controller for an endpoint that belongs to an existing workflow — add a method instead

Keep methods small and independently testable.

---

## Logging

Log in operations:
- Operation start (include identifying fields, e.g. `Email`, `UploadId`)
- Key business events (e.g. session created, job queued)
- Downstream failures (log as Warning or Error before re-throwing)

Never log:
- Presigned POST fields or URLs
- Secrets or tokens
- Full request/response payloads

---

## Validation

Validate inside the operation before calling any service. Throw `ArgumentException` for missing or invalid input — the controller catches it and returns 400.

Required fields to validate:
- `Email` — must be non-empty
- `UploadId` — must be non-empty when present
- File metadata (`ContentType`, `Size > 0`) when processing file uploads

---

## Configuration

Two configuration files only:
- `appsettings.local.json`
- `appsettings.production.json`

Do not add additional environment files.

Configured values (bound to `AppSettings`):
- `App:S3BucketName`
- `App:SqsQueueUrl`
- `App:DynamoDbTableName`
- `App:AwsRegion`

---

## What Claude Must Do

- Read the operation's `.instruction/business.md` before writing any business logic
- Read `.instruction/*.request.json` and `.instruction/*.response.json` before writing DTOs
- Register new operations in `ServiceCollectionExtensions`
- Add a controller for every new operation
- Follow the three-layer structure: Controller → Operation → Service

## What Claude Must Avoid

- Adding endpoints not listed in this document
- Placing AWS SDK calls directly in an operation
- Placing business logic in a service
- Creating a top-level `/api-contracts/` folder (contracts live in `.instruction/`)
- Adding extra `appsettings.*.json` files
- Adding unnecessary architecture layers or frameworks
