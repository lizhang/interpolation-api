Spawn a sub-agent to review the current git changes as a senior software developer and write the findings to a timestamped file.

## Step 1 — Get changed files

Run `git diff --name-only HEAD` to list changed files. If the working tree has unstaged changes, also run `git diff --name-only` to catch them.

## Step 2 — Spawn the sub-agent

Use the Agent tool with this prompt (fill in the changed file list from Step 1):

---

**Sub-agent prompt:**

You are a senior software developer reviewing a pull request on a C# .NET 10 ASP.NET Core backend (AWS Lambda + S3 + SQS + DynamoDB).

**Changed files:**
{list from Step 1}

**Your task:** Review only the changed files. Produce a written review covering correctness, architecture, and any violations.

---

### Pre-search protocol — do this before reading any source file

1. Read `.ai/context/backend-file-map.md`
2. Read `.ai/context/file-dependencies.md`
3. From those maps, identify which changed files are worth reading in full vs. skimming. Do not read files not in the changed list unless a dependency is directly relevant to understanding a violation.

---

### Architecture rules to enforce (always apply)

Controller layer:
- Must only handle routing, DTO binding, response mapping, and HTTP status — no business logic
- Must delegate all logic to an Operation

Operation layer:
- Contains all business logic and workflow orchestration
- Must not call infrastructure (DB, S3, SQS) directly — must go through Services
- Must not inject `IConfiguration` or `AppSettings` directly
- Methods over ~200 lines should be decomposed

Service layer:
- Handles infrastructure concerns only (DB, API, external systems)
- Must be generic and reusable — no domain-specific method names (e.g. `SaveUploadSessionAsync` is not allowed)
- Must not accept or return domain model types (e.g. `JobRecord`)
- No business logic

All Operations and Services:
- Must have interfaces
- Must be registered in DI

Logging (in Operations only):
- Use structured logging: `_logger.LogInformation("... {Field}", value)`
- Log: operation start with identifying fields, key business events, downstream failures as Warning/Error before re-throwing
- Never log: presigned URLs, secrets, tokens, full request/response payloads

---

### Configuration rules (apply only if any changed file is `AppSettings.cs`, `appsettings*.json`, or a Service that reads config)

- All config bound under `"App"` section via `IOptions<AppSettings>` — no raw `IConfiguration` access
- No hardcoded values (bucket names, queue URLs, table names, region strings, ARNs) in C# source
- Any new `AppSettings` property must have a matching entry in both `appsettings.local.json` and `appsettings.production.json`
- No secrets or credentials in any config file

---

### Output format

Write the review to `.\review\{YYYYMMDD-HHmmss}_review.txt` (use the current date/time for the filename).

Structure:

```
REVIEW — {date-time}
Changed files: {list}

## Summary
{1-3 sentence overall assessment}

## File-by-file findings
### {filename}
- {finding}: {explanation and line reference if applicable}
...

## Violations
{List any architecture or configuration rule violations. For each: rule broken, file, line if known, suggested fix.}

## Approved
{List files with no findings.}
```

If there are no violations, state that explicitly in the Violations section.

---

## Step 3 — Confirm

After the sub-agent completes, report the path of the written review file.
