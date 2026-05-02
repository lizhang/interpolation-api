---
name: operation-from-instructions
description: Generate a complete operation (domain objects, interface, implementation, controller method, DI) from an operation's `.instruction/` folder.
---

# Skill: Operation From Instructions

## Purpose

Generate a complete operation (domain objects, interface, implementation, controller method, DI) from an operation's `.instruction/` folder.

Follows `backend-architecture` skill for all layer rules.

---

## When to use

- Add a new operation / endpoint
- Implement backend logic from a `.instruction/` folder

---

## Required inputs

Before writing any code, read [`STRUCTURE.md`](../../../STRUCTURE.md) for the current file layout, then:

1. `<OperationFolder>/.instruction/business.md` — route, HTTP method, validation rules, steps, error cases
2. `<OperationFolder>/.instruction/request.json` — request shape
3. `<OperationFolder>/.instruction/response.json` — response shape
4. Existing controller(s) to determine where to add the endpoint
5. `src/Extensions/ServiceCollectionExtensions.cs` for DI registration style

Stop and report if any instruction file is missing.

---

## Naming conventions

| Artifact | Name | Location |
|---|---|---|
| Domain input | `<Name>Input.cs` | `src/Operations/<Name>/` |
| Domain result | `<Name>Result.cs` | `src/Operations/<Name>/` |
| Interface | `I<Name>Operation.cs` | `src/Operations/<Name>/` |
| Implementation | `<Name>Operation.cs` | `src/Operations/<Name>/` |
| Request DTO | `<Name>Request.cs` | `src/Controllers/Dtos/` |
| Response DTO | `<Name>Response.cs` | `src/Controllers/Dtos/` |

Operation method signature:
```csharp
Task<NameResult> ExecuteAsync(NameInput input, CancellationToken ct);
```

---

## Instruction file meanings

| File | Defines |
|---|---|
| `business.md` | Business logic, validation rules, step sequence, error cases |
| `request.json` | Exact fields for `<Name>Request.cs` (DTO) and `<Name>Input.cs` (domain) |
| `response.json` | Exact fields for `<Name>Response.cs` (DTO) and `<Name>Result.cs` (domain) |

Do not invent fields not present in the contract files.

---

## Implementation steps

1. Read `business.md`, `request.json`, `response.json`
2. Create `<Name>Input.cs` and `<Name>Result.cs` (domain objects)
3. Create `I<Name>Operation.cs` and `<Name>Operation.cs`
4. Create `<Name>Request.cs` and `<Name>Response.cs` in `src/Controllers/Dtos/`
5. Add endpoint to the appropriate controller — prefer adding to an existing controller if the domain matches; create a new one only if truly separate
6. In the controller method: map DTO → Input, call operation, map Result → DTO
7. Register operation in `ServiceCollectionExtensions.cs`

---

## Error handling

| Exception | HTTP status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 404 Not Found |
| Unhandled | 500 (framework default) |
