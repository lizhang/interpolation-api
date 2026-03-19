# Write Operation Skill

This skill defines how to implement a new operation in the frame interpolation backend.

Operations live in:

src/Operations

Each operation must have its own folder.

Example

Operations/SubmitJob

Required files

I<OperationName>Operation.cs  
<OperationName>Operation.cs  
<OperationName>Request.cs  
<OperationName>Response.cs

---

## Steps

1 Read API contract from `/api-contracts`.

2 Create request DTO.

3 Create response DTO.

4 Create operation interface.

Example

Task<Response> ExecuteAsync(Request request, CancellationToken ct)

5 Implement operation.

Responsibilities

- validate request
- call services
- log events
- return response DTO

Operations must NOT call AWS SDK directly.

---

## Services

Operations must use services such as

IS3Service  
ISqsService  
IDynamoDbService

Services wrap AWS SDK usage.

---

## Validation

Validate required fields before calling services.

Example

- email
- uploadId
- file metadata

---

## Logging

Log operation start and important events.

Avoid logging sensitive information.

---

## Dependency Injection

Operations must be registered in the DI container.

---

## Code Style

Use

- async / await
- constructor injection
- clear naming
- small methods

Avoid unnecessary abstraction.