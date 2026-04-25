# Good Hamburger

Technical challenge solution for a snack bar ordering system using `.NET 10`, `ASP.NET Core Web API`, and `Blazor Server`.

## Overview

The solution implements:

- Menu endpoint
- Full order CRUD
- Discount rules
- Business validations with clear error messages
- Blazor frontend consuming the API
- Automated business rule tests
- GitHub Actions CI pipeline
- Structured logging with correlation ID support

## Solution Structure

- `GoodHamburger.API` → REST API layer (controllers + middleware)
- `GoodHamburger.Application` → use cases, services, contracts, business orchestration
- `GoodHamburger.Domain` → entities and enums
- `GoodHamburger.Infrastructure` → in-memory repository and static menu catalog
- `GoodHamburger.Web` → Blazor frontend
- `GoodHamburger.Test` → business rule automated tests
- `GoodHamburger.API.IntegrationTests` → API integration tests for REST endpoints

## Business Rules

### Menu

- Sandwiches:
  - `1` - X Burger - `5.00`
  - `2` - X Egg - `4.50`
  - `3` - X Bacon - `7.00`
- Sides:
  - `4` - French fries - `2.00`
  - `5` - Soda - `2.50`

### Discounts

- Sandwich + fries + soda → `20%`
- Sandwich + soda → `15%`
- Sandwich + fries → `10%`

### Validations

- Order must contain exactly one sandwich
- Order can contain at most one fries
- Order can contain at most one soda
- Duplicated items are rejected
- Unknown item IDs are rejected

## API Endpoints

Base route: `/api`

### Menu

- `GET /api/menu`

### Orders

- `GET /api/orders`
- `GET /api/orders/{id}`
- `POST /api/orders`
- `PUT /api/orders/{id}`
- `DELETE /api/orders/{id}`

### Request Payload (create/update)

```json
{
  "itemIds": [1, 4, 5]
}
```

## Security

### Path Traversal Protection

The Web application includes middleware protection against path traversal attempts.

- Suspicious patterns such as `../`, `/..`, `\\..\\`, and URL-encoded variants are detected.
- When detected, the request is blocked and redirected to Home (`/`).
- The validation is implemented in `GoodHamburger.Web/Program.cs`.

## Running Locally

## Prerequisites

- `.NET 10 SDK`
- Visual Studio 2026 or `dotnet` CLI

### 1) Run API

From workspace root:

```powershell
dotnet run --project GoodHamburger.API/GoodHamburger.API.csproj
```

Default API URL (launch profile):

- `https://localhost:7228`
- `http://localhost:5008`

Swagger:

- `https://localhost:7228/swagger`

### 2) Run Web

```powershell
dotnet run --project GoodHamburger.Web/GoodHamburger.Web.csproj
```

Default Web URL (launch profile):

- `https://localhost:7002`
- `http://localhost:5216`

### 3) Configure frontend API URL

`GoodHamburger.Web/appsettings.Development.json`:

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7228"
  }
}
```

## Run Tests

```powershell
dotnet test GoodHamburger.Test/GoodHamburger.Test.csproj
dotnet test GoodHamburger.API.IntegrationTests/GoodHamburger.API.IntegrationTests.csproj
```

## CI

GitHub Actions workflow: `.github/workflows/ci.yml`

Pipeline steps:

- Restore
- Build
- Test (`GoodHamburger.Test`) with TRX output
- Test (`GoodHamburger.API.IntegrationTests`) with TRX output
- Collect code coverage (`XPlat Code Coverage`)
- Upload `TestResults` as workflow artifact

## Architecture Decisions

- Layered architecture (`Domain`, `Application`, `Infrastructure`, `API`) for clear separation of concerns.
- In-memory repository for challenge simplicity and quick validation.
- Static menu catalog to keep deterministic business behavior.
- Domain entities kept minimal; business rules orchestrated in `OrderService`.
- Global API exception middleware for consistent error response format.
- Correlation ID middleware (`X-Correlation-ID`) with JSON structured logs and traceable error payloads.

## What Was Intentionally Left Out

- Persistent database (SQL/NoSQL) - The in-memory repository suffices for the challenge scope and allows for faster development and testing. In a production scenario, I would implement a proper data access layer with an ORM and a relational database.
- Authentication/authorization - This was not specified in the requirements and would add unnecessary complexity for the challenge scope.
- Full UX refinement in Blazor - The Blazor frontend focuses on functionality and API integration rather than polished UI/UX, given the challenge scope and time constraints.


# Author
- GitHub: [DevJoaoVitorBP](https://github.com/DevJoaoVitorBP)
- LinkedIn: [devjoaopereira](https://www.linkedin.com/in/devjoaopereira)

# Final notes
This solution demonstrates a clean architecture approach with a focus on maintainability, testability, and adherence to the specified business rules. The layered structure allows for clear separation of concerns, making it easier to extend and modify the application in the future. The use of structured logging and correlation IDs enhances observability and debugging capabilities.
