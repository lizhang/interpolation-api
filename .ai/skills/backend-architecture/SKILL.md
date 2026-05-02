---
name: backend-architecture
description: Backend Architecture Enforcement
---

# Skill: Backend Architecture Enforcement (C# Service)

## Purpose

Enforce consistent backend architecture patterns for C# services, including Controllers, Operations, and Services separation of concerns.

---

## Repository structure

Read [`STRUCTURE.md`](../../../STRUCTURE.md) before generating or refactoring any code.

---

## When to use

Apply this skill when:

* Generating new backend code
* Refactoring existing controllers, operations, or services
* Reviewing API structure or responsibilities

---

## Architecture Rules

### 1. Controller Responsibilities

* Responsible ONLY for:

  * Route registration
  * Request parsing (DTO binding)
  * Response mapping
  * HTTP status / error handling
* MUST NOT contain business logic
* SHOULD remain thin and simple

#### Controller Organization

* Group endpoints by domain
* Example:

  * JobsController handles all job-related endpoints
* Split controllers ONLY if file becomes too large

---

### 2. Operation Layer (Business Logic)

* Controllers MUST call Operations
* Operations:

  * Contain ALL business logic
  * Orchestrate workflows
* Keep methods small and readable
* If method exceeds ~200 lines:

  * Extract into private helper methods

---

### 3. Service Layer (Infrastructure Access)

* Operations call Services
* Services:

  * Handle infrastructure concerns (DB, API, external systems)
  * SHOULD be generic and reusable
  * No domain-specific method names (e.g. `SaveUploadSessionAsync` is not allowed)
  * No domain model types (`JobRecord`, etc.) as parameters or returns
* Avoid embedding business logic in services

---

### 4. Interfaces and Dependency Injection

* ALL Operations and Services MUST:

  * Have interfaces
  * Be registered via Dependency Injection
* Use constructor injection

---

### 5. Code Quality Rules

* Enforce separation of concerns strictly
* Avoid tight coupling between layers
* Prefer composition over large monolithic methods
* Keep code testable and modular

---

## Output Expectations

When generating or refactoring code:

* Follow Controller → Operation → Service flow strictly
* Reject designs that violate separation of concerns
* Suggest refactoring when logic is misplaced

---

## Anti-Patterns (DO NOT DO)

* Business logic inside Controllers
* Direct DB/API calls inside Controllers
* Fat Services with business logic
* Large methods without decomposition
* Missing interfaces for Operations/Services

---

## Example Flow

Controller → Operation → Service → Infrastructure