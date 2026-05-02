# File Dependencies Reference

This file is a navigation aid only.
Always verify dependencies in actual code before editing.

## JobsController.cs
Usually depends on:
- Controllers/Dtos/*
- Operations/*/I*Operation.cs

## Operations/<OperationName>/
Usually contains:
- I<OperationName>Operation.cs
- <OperationName>Operation.cs
- <OperationName>Input.cs
- <OperationName>Result.cs
- .instruction/business.md
- .instruction/*.request.json
- .instruction/*.response.json

Usually depends on:
- Services/I*.cs
- Shared domain models if needed

## Services/
Usually depends on:
- Infrastructure SDKs
- DB clients
- S3 clients
- Queue/message clients

Should not depend on:
- Controllers
- DTOs
- Operation-specific HTTP models