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

Business logic for each endpoint lives in its operation's `.instruction/business.md`.

---

## Architecture

Follows the `backend-architecture` skill.

---

## Configuration

Follows the `configuration` skill. No hardcoded configuration values (bucket names, queue URLs, table names, region strings, ARNs) anywhere in C# source files.

---

## Commands

| Command | Usage |
|---|---|
| `/operation-from-instructions <OperationFolder>` | Generate a complete operation (domain objects, interface, implementation, controller method, DI) from an `.instruction/` folder |
| `/unit-test-writer <ClassName>.<MethodName>` | Generate MSTest unit tests using Moq for a specific method |
| `/code-review` | Spawn a sub-agent to review changed files against architecture and configuration rules; writes findings to `.\review\{datetime}_review.txt` |

---
