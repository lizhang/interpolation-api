---
name: configuration
description: Configuration conventions for the InterpolationApi backend
---

# Skill: Configuration

## Config files

Two environment-specific files exist — do not add others:

| File | Environment |
|---|---|
| `appsettings.local.json` | Local development |
| `appsettings.production.json` | Production |

`appsettings.json` is the base file (shared defaults, no environment-specific values).

---

## AppSettings binding

All configuration is bound to `AppSettings` under the `"App"` section:

```csharp
services.Configure<AppSettings>(configuration.GetSection("App"));
```

The class lives at `src/InterpolationApi/Configuration/AppSettings.cs` and exposes:

| Property | Config key |
|---|---|
| `S3BucketName` | `App:S3BucketName` |
| `SqsQueueUrl` | `App:SqsQueueUrl` |
| `DynamoDbTableName` | `App:DynamoDbTableName` |
| `AwsRegion` | `App:AwsRegion` |

---

## How to consume

Inject via `IOptions<AppSettings>` in Services:

```csharp
public MyService(IOptions<AppSettings> settings) => _settings = settings.Value;
```

Operations do NOT inject `AppSettings` directly — they call Services, which own the infrastructure concern.

---

## Hard constraints

- **No `IConfiguration` in Operations or Services directly** — use `IOptions<AppSettings>`.
- **No hardcoded configuration values** (bucket names, queue URLs, table names, region strings, account IDs, ARNs) anywhere in C# source files.
- **No new `AppSettings` properties** without a matching entry in both `appsettings.local.json` and `appsettings.production.json`.
- **No secrets or credentials** in any config file — use IAM roles / environment injection.

---

## Adding a new config value

1. Add the property to `AppSettings.cs`.
2. Add the key under `"App"` in `appsettings.local.json` and `appsettings.production.json`.
3. Consume via `IOptions<AppSettings>` in the Service that needs it.

---

## Anti-patterns (DO NOT DO)

- `var bucket = "my-bucket";` — hardcoded value in C# source
- `_configuration["App:S3BucketName"]` — raw `IConfiguration` access inside an operation or service
- A third config file (e.g. `appsettings.Development.json`) — only `local` and `production` are allowed
- Putting AWS account IDs, SQS URLs, or ARNs directly in source code
