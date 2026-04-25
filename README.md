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

## Architecture

The solution follows a layered architecture (Clean Architecture inspired):

| Layer | Responsibility |
|------|--------------|
| API | HTTP layer (controllers + middleware) |
| Application | Business orchestration and use cases |
| Domain | Core entities and rules |
| Infrastructure | Data access (in-memory) + catalog |
| Web (Blazor) | Frontend |
| Tests | Unit and integration tests |

### Projects

- GoodHamburger.API  
- GoodHamburger.Application  
- GoodHamburger.Domain  
- GoodHamburger.Infrastructure  
- GoodHamburger.Web  
- GoodHamburger.Test  
- GoodHamburger.API.IntegrationTests  

### Business Rules

### Menu

| ID | Item | Price |
|----|------|------|
| 1 | X Burger | 5.00 |
| 2 | X Egg | 4.50 |
| 3 | X Bacon | 7.00 |
| 4 | French Fries | 2.00 |
| 5 | Soda | 2.50 |


### Discounts

| Combination | Discount |
|------------|---------|
| Sandwich + Fries + Soda | 20% |
| Sandwich + Soda | 15% |
| Sandwich + Fries | 10% |


### Validations

- Must contain exactly 1 sandwich  
- Max 1 fries  
- Max 1 soda  
- Duplicated items are rejected  
- Unknown item IDs are rejected 

## API

Base route: `/api`

---

### Menu

```
GET /api/menu
```

---

### Orders

#### Get all orders

```
GET /api/orders
```

Response:

```json
[
  {
    "id": 1,
    "items": [
      {
        "id": 1,
        "name": "X Burger",
        "category": "Sandwich",
        "price": 5.00
      }
    ],
    "subtotal": 5.00,
    "discountPercentage": 0,
    "discountAmount": 0,
    "total": 5.00,
    "createdAt": "2026-04-25T16:32:58.806Z",
    "updatedAt": "2026-04-25T16:32:58.806Z"
  }
]
```

---

#### Get order by ID

```
GET /api/orders/{id}
```

Responses:
- 200 OK  
- 404 Not Found  

---

#### Create order

```
POST /api/orders
```

Request:

```json
{
  "itemIds": [1, 4, 5]
}
```

Response:
- 201 Created  

---

#### Update order

```
PUT /api/orders/{id}
```

Request:

```json
{
  "itemIds": [1, 5]
}
```

Response:

```json
{
  "id": 1,
  "items": [
    {
      "id": 1,
      "name": "X Burger",
      "category": "Sandwich",
      "price": 5.00
    },
    {
      "id": 5,
      "name": "Soda",
      "category": "Drink",
      "price": 2.50
    }
  ],
  "subtotal": 7.50,
  "discountPercentage": 15,
  "discountAmount": 1.125,
  "total": 6.375,
  "createdAt": "2026-04-25T16:33:32.954Z",
  "updatedAt": "2026-04-25T16:33:32.954Z"
}
```

---

#### Delete order

```
DELETE /api/orders/{id}
```

Response:
- 204 No Content  
- 404 Not Found  

---

### Error Response Pattern

```json
{
  "type": "string",
  "title": "string",
  "status": 400,
  "detail": "string",
  "instance": "string"
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
