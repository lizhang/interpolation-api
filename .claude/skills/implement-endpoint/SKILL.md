# Implement Endpoint Skill

Implements a new endpoint from its `.instruction/` spec.

---

## Steps

1. Read `src/Operations/<OperationName>/.instruction/business.md` — authoritative spec.

2. Read `.instruction/*.request.json` and `.instruction/*.response.json` contracts.

3. Create domain objects in `src/Operations/<OperationName>/`:
   - `<OperationName>Input.cs` — fields derived from the request contract (no "Request" suffix)
   - `<OperationName>Result.cs` — fields derived from the response contract (no "Response" suffix)

4. Create `I<OperationName>Operation.cs` and `<OperationName>Operation.cs`:
   - Interface: `Task<OperationNameResult> ExecuteAsync(OperationNameInput input, CancellationToken ct)`
   - Implementation: validate input, call services, log events, return result

5. Create DTOs in `src/Controllers/Dtos/`:
   - `<OperationName>Request.cs` — matches the request contract exactly
   - `<OperationName>Response.cs` — matches the response contract exactly

6. Add the endpoint to `JobsController` if it belongs to the job workflow, or create a new controller only if the domain is truly separate. No class-level `[Route]` when the controller owns multiple URL prefixes — use full paths on each `[HttpVerb("api/...")]` method.
   - Map DTO → domain input, call operation, map result → response DTO
   - Map `ArgumentException` → 400, `InvalidOperationException` → 404

7. Register the operation in `src/Extensions/ServiceCollectionExtensions.cs`.

---

## Rules

- Operations only see domain objects — never import from `Controllers.Dtos`
- DTOs live in `src/Controllers/Dtos/` — never in the operation folder
- DTOs must exactly match the `.instruction/` contracts — do not invent fields
- Validate in the operation (throw `ArgumentException`), not in the controller
- Operations must NOT call AWS SDK directly — use service interfaces
- Extract `private` methods when an operation method body exceeds 200 lines

### Service rules (do not violate these when adding operations)

- **Never** add domain-specific methods to service interfaces (e.g. `SaveXxxAsync`, `GetJobAsync`)
- **Never** use domain model types (`JobRecord`, etc.) as service parameters or returns
- Use `IDynamoDbService` generic methods: `PutItemAsync`, `GetItemAsync`, `UpdateItemAsync`, `IncrementCounterAsync`, `IncrementAndGetAsync`
- Use `ISqsService.SendMessageAsync<T>` — pass any serializable message type
- The operation owns all domain knowledge: which pk/sk to use, which attribute names, which counter fields
