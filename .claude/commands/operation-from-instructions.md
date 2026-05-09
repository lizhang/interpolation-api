Generate a complete operation from the instruction folder: $ARGUMENTS

## Prerequisites

Check that `$ARGUMENTS/.instruction/business.md` exists. If it does not, stop and tell the user to create it before running this command.

---

## Step 1 — Read inputs

Read all four sources before writing any code:

1. `$ARGUMENTS/.instruction/business.md` — route, HTTP method, validation rules, steps, error cases
2. `$ARGUMENTS/.instruction/request.json` — request shape (source of truth for field names/types)
3. `$ARGUMENTS/.instruction/response.json` — response shape (source of truth for field names/types)
4. `src/Extensions/ServiceCollectionExtensions.cs` — DI registration style

If `request.json` or `response.json` is missing, stop and report which file is absent.

Do not invent fields not present in the contract files.

---

## Step 2 — Determine operation name

Derive `<Name>` from the folder name at `$ARGUMENTS` (e.g. `src/Operations/SubmitJob` → `SubmitJob`).

---

## Step 3 — Create domain objects

| File | Location |
|---|---|
| `<Name>Input.cs` | `src/Operations/<Name>/` |
| `<Name>Result.cs` | `src/Operations/<Name>/` |

Fields come strictly from `request.json` (Input) and `response.json` (Result).

---

## Step 4 — Create operation interface and implementation

| File | Location |
|---|---|
| `I<Name>Operation.cs` | `src/Operations/<Name>/` |
| `<Name>Operation.cs` | `src/Operations/<Name>/` |

Method signature:

```csharp
Task<{Name}Result> ExecuteAsync({Name}Input input, CancellationToken ct);
```

Implement all steps, validation rules, and error cases from `business.md`.

Error mapping:

| Exception | HTTP status |
|---|---|
| `ArgumentException` | 400 Bad Request |
| `InvalidOperationException` | 404 Not Found |
| Unhandled | 500 (framework default) |

---

## Step 5 — Create DTOs

| File | Location |
|---|---|
| `<Name>Request.cs` | `src/Controllers/Dtos/` |
| `<Name>Response.cs` | `src/Controllers/Dtos/` |

Fields come strictly from `request.json` (Request) and `response.json` (Response).

---

## Step 6 — Add controller endpoint

Prefer adding to an existing controller if the domain matches; create a new one only if truly separate.

In the method body:
1. Map `<Name>Request` → `<Name>Input`
2. Call `await _operation.ExecuteAsync(input, ct)`
3. Map `<Name>Result` → `<Name>Response`

---

## Step 7 — Register in DI

Add the operation to `src/Extensions/ServiceCollectionExtensions.cs` following the existing registration style.
