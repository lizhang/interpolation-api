# CLAUDE.md - API 

## Project Overview

This project is the backend for a **frame interpolation web application**.

Users upload two key frames and the system generates in-between frames using an AI model.

The backend:

- generates **S3 presigned upload URLs**
- accepts **job submissions**
- writes **job metadata to DynamoDB**
- queues processing jobs in **SQS**

The service is built using:

- .NET 10
- AWS Lambda
- Amazon S3
- Amazon SQS
- Amazon DynamoDB

The backend intentionally exposes **three endpoints**.

---

# Repository Structure

The repository follows this structure.

project-root
│
├─ CLAUDE.md
│
├─ api-contracts
│ ├─ uploads.request.json
│ ├─ uploads.response.json
│ ├─ jobs.request.json
│ └─ jobs.response.json
│
├─ .claude
│ └─ skills
│ └─ write-operation
│ └─ SKILL.md
│
└─ src
├─ Operations
│ ├─ GeneratePresignedUrls
│ └─ SubmitJob
│
├─ Services
│
├─ Models
│
├─ Configuration
│
├─ Extensions
│
├─ Program.cs
│
├─ appsettings.local.json
└─ appsettings.production.json


Claude must follow this structure when generating code.

---

# API Contract Source

The canonical API contracts are stored in the **`/api-contracts`** directory.

Claude **must treat these JSON files as the authoritative request and response schemas** when generating DTO classes.

Contracts:
api-contracts/uploads.request.json
api-contracts/uploads.response.json
api-contracts/jobs.request.json
api-contracts/jobs.response.json


When implementing an operation:

- read the contract JSON
- generate request/response DTOs that match the schema
- do not invent additional fields unless required

---

# Endpoints

The system supports exactly **three endpoints**.

---

## 0 Health Check

Endpoint
    GET /api/health

### Behavior

Returns HTTP 200 with `{ "ok": true }`.

No operation class is required. The endpoint is mapped directly in `Program.cs`.

---

## 1 Generate Presigned Upload URLs

Endpoint
    POST /api/uploads
    Request schema is defined in: api-contracts/uploads.request.json
    Response schema is defined in: api-contracts/uploads.response.json
    
### Behavior

The operation must:

1. validate request payload
2. generate a unique `uploadId`
3. generate S3 keys for both files
4. create **presigned POST uploads**
5. return upload metadata

### S3 key format
uploads/{uploadId}/start.png
uploads/{uploadId}/end.png

### Required responsibilities

- generate `uploadId`
- generate presigned POST upload data
- store upload session metadata

Upload session metadata must include:
email
uploadId
startFrameKey
endFrameKey
createdAt


The upload session must be persisted so it can be used later by `/api/jobs`.

---

## 2 Submit Job

Endpoint
POST /api/jobs
Request schema:
api-contracts/jobs.request.json
Response schema:
api-contracts/jobs.response.json

### Behavior

This operation must:

1. validate request
2. resolve upload session
3. generate `jobId`
4. write job record to DynamoDB
5. publish job message to SQS
6. return queued response

---

# DynamoDB Requirements

A DynamoDB job record must be written when a job is submitted.

### Table Keys
Partition Key: email
Sort Key: jobId

### Required attributes
email
jobId
startFrameKey
endFrameKey
createdAt
resultsJson


Example conceptual record
{
email: "user@example.com
",
jobId: "job_abc",
startFrameKey: "uploads/upl_123/start.png",
endFrameKey: "uploads/upl_123/end.png",
createdAt: "2026-03-06T10:00:00Z",
resultsJson: ""
}


Notes

- `createdAt` must be UTC
- `resultsJson` must initially be empty

---

# SQS Message

When submitting a job, publish a message containing:
jobId
email
uploadId
startFrameKey
endFrameKey
createdAt


This message will later be processed by the AI interpolation worker.

---

# Architecture Rules

The backend must follow a **three-layer structure**.
endpoint → operation → service → AWS SDK

Operations coordinate logic.

Services wrap infrastructure.

---

# Operations Layer

Operations live under:
src/Operations

Each operation must have its **own folder**.

Example
Operations/
GeneratePresignedUrls/
IGeneratePresignedUrlsOperation.cs
GeneratePresignedUrlsOperation.cs
GeneratePresignedUrlsRequest.cs
GeneratePresignedUrlsResponse.cs


Responsibilities

Operations must:

- validate request
- coordinate business logic
- call services
- return response DTOs

Operations must **not contain raw AWS SDK logic**.

---

# Services Layer

Services wrap AWS SDK functionality.

All infrastructure access must be implemented inside `Services`.

Example services
IS3Service
S3Service

ISqsService
SqsService

IDynamoDbService
DynamoDbService

Responsibilities

S3Service

- generate presigned POST uploads

SqsService

- send job messages

DynamoDbService

- write job records
- read upload sessions

---

# Configuration

The application must only support **two configuration environments**.
appsettings.local.json
appsettings.production.json


Do not add additional configuration files unless explicitly required.

Typical settings
S3 bucket name
SQS queue URL
DynamoDB table name
AWS region


---

# Coding Rules

Claude should follow these coding rules.

Use

- async / await
- constructor dependency injection
- explicit DTO models
- strongly typed configuration
- structured logging

Avoid

- unnecessary frameworks
- repository pattern
- CQRS frameworks
- over-engineering

Keep methods:

- small
- readable
- testable

---

# Logging

Operations should log

- start of operation
- important business events
- downstream service failures

Never log

- presigned POST fields
- secrets
- sensitive payloads

---

# Validation

Operations must validate:

- required fields
- email presence
- file metadata
- uploadId presence

Return clear validation errors.

---

# Skills

Claude skills are located under:
.claude/skills
The project includes a skill for writing operations.
.claude/skills/write-operation/SKILL.md

Claude should follow this skill whenever implementing new operations.

---

# What Claude Should Do

When asked to implement an endpoint:

1. read API contracts in `/api-contracts`
2. create operation folder
3. generate DTOs matching contract
4. create operation interface
5. create implementation
6. call service wrappers
7. register dependency injection
8. map endpoint

---

# What Claude Must Avoid

Claude must NOT:

- add endpoints not listed in this document
- bypass service layer
- add unnecessary architecture patterns
- add additional environment configuration files
- mix AWS SDK code directly into operations

---

# Design Goal

The design goal is a **simple, maintainable, serverless backend** with clear separation between:

- HTTP layer
- business operations
- infrastructure services

The codebase should remain easy to extend and easy to reason about.
