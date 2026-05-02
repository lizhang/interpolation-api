---
name: unit-test-writer
description: Generate MSTest unit tests using Moq and built-in assertions
---

# Skill: MSTest Unit Test Writer

## Purpose

Generate focused C# unit tests using:

* MSTest
* Moq
* Built-in MSTest assertions

Do not use xUnit, NUnit, FluentAssertions, Shouldly, or other assertion libraries unless explicitly requested.

---

## Memory reset rule

Before starting a new unit test task:

* Use `/clear` to start a new session
* Do not rely on previous context, assumptions, or earlier generated tests
* Treat the current request as the only source of truth

---

## When to use

Use this skill when the user asks to:

* Add unit tests
* Improve test coverage
* Test a specific method
* Test a controller, operation, or service
* Generate tests from JSON test case definitions

---

## Required user input

If the user does NOT provide BOTH:

* target file/class
* target method

Then ask the user:

```
Please provide the file/class and method you want tested.
```

Do not proceed until both are provided.

---

## Core test rules

Always:

* Cover success path
* Cover important failure and edge cases
* Mock external dependencies
* Avoid testing private methods directly
* Test behavior, not implementation details
* Follow existing project naming and structure conventions

---

## If user provides method but no test case file

Steps:

1. Locate the class containing the method
2. Read the method implementation
3. Identify:

   * Inputs
   * Outputs
   * Dependencies (constructor injected)
   * Validation rules
   * Branches
   * Error cases
   * CancellationToken usage if applicable
4. Generate focused tests only for that method
5. Mock external dependencies using Moq
6. Use MSTest built-in assertions

---

## If user provides test case file

Test case file format:

```json
[
  {
    "name": "valid request returns success",
    "input": {
      "jobId": "job-123"
    },
    "output": {
      "success": true
    }
  }
]
```

Steps:

1. Inspect the test case file
2. Understand each input/output pair
3. Map JSON input to method request model
4. Map JSON output to expected assertions
5. Generate one test per case (unless project uses parameterized tests)
6. Do NOT override user-defined expected results
7. If setup is missing, infer from code or mention assumptions

---

## MSTest conventions

Example:

```csharp
[TestClass]
public class SubmitJobOperationTests
{
    [TestMethod]
    public async Task ExecuteAsync_WhenRequestIsValid_ShouldReturnExpectedResult()
    {
        // Arrange

        // Act

        // Assert
    }
}
```

Async exception test:

```csharp
await Assert.ThrowsExceptionAsync<SomeException>(async () =>
{
    await sut.ExecuteAsync(request, cancellationToken);
});
```

Assertions:

```csharp
Assert.IsNotNull(result);
Assert.AreEqual(expected, actual);
Assert.IsTrue(condition);
Assert.IsFalse(condition);
```

---

## Moq rules

Mock all external dependencies injected via constructor.

Example:

```csharp
var mock = new Mock<IJobService>();
```

Verify only important calls:

```csharp
mock.Verify(x => x.SubmitAsync(It.IsAny<SubmitJobInput>(), It.IsAny<CancellationToken>()), Times.Once);
```

Avoid over-verification.

---

## Test file location

Follow existing project convention.

If none exists:

```
InterpolationApiTest/<Layer>/<ClassName>Tests.cs
```

Examples:

* Operations → SubmitJobOperationTests.cs
* Controllers → JobsControllerTests.cs
* Services → S3ServiceTests.cs

---

## Controller test rules

Verify:

* DTO → Operation mapping
* Operation result → Response mapping
* Correct HTTP status/result
* Error mapping

Do NOT:

* Test business logic
* Mock infrastructure

---

## Operation test rules

Verify:

* Business logic
* Validation
* Branches
* Error handling
* Service orchestration
* Returned results

Mock:

* Services
* Repositories
* External APIs
* Storage/queue systems

---

## Service test rules

Verify:

* Infrastructure interaction
* SDK/client calls
* Error handling
* Retry/fallback if exists

Mock:

* SDK clients
* HTTP/DB clients

Do NOT call real infrastructure.

---

## Private methods

Do NOT test private methods directly.

Test through public methods.

---

## Output behavior

When generating tests:

1. State target method
2. State source:

   * implementation analysis OR
   * JSON test cases
3. Create/update test file
4. Summarize test coverage
5. Mention assumptions if any

---

## Anti-patterns

Do NOT:

* Use non-MSTest frameworks
* Use non-Moq mocking libraries
* Use external assertion libraries
* Call real infrastructure
* Test private methods directly
* Duplicate business logic in tests
* Over-mock trivial objects
* Ignore project conventions
