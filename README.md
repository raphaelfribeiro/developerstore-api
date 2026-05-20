# DeveloperStore API

<div align="center">

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=for-the-badge&logo=docker&logoColor=white)

![Entity Framework](https://img.shields.io/badge/Entity_Framework-Core_8-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MediatR](https://img.shields.io/badge/MediatR-12.4-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![AutoMapper](https://img.shields.io/badge/AutoMapper-15.1.3-BE3C28?style=for-the-badge&logo=dotnet&logoColor=white)
![FluentValidation](https://img.shields.io/badge/FluentValidation-11.x-00B4AB?style=for-the-badge&logo=dotnet&logoColor=white)

![xUnit](https://img.shields.io/badge/xUnit-2.9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Coverage](https://img.shields.io/badge/Coverage-93%25-brightgreen?style=for-the-badge&logo=dotnet&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)
![Serilog](https://img.shields.io/badge/Serilog-8.x-EF5B25?style=for-the-badge&logo=dotnet&logoColor=white)

![CI](https://github.com/raphaelfernandesribeiro/desafio-tecnico/actions/workflows/ci.yml/badge.svg?branch=develop)
[![Release](https://img.shields.io/github/v/release/raphaelfernandesribeiro/desafio-tecnico?style=for-the-badge&logo=github&logoColor=white&label=Release)](https://github.com/raphaelfernandesribeiro/desafio-tecnico/releases/latest)

</div>

---

<div align="center">

> **Technical Challenge — Developer Evaluation**
>
> RESTful API for managing sales, carts and products with a DDD and CQRS architecture.

</div>

---

## Table of Contents

- [Overview](#overview)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Business Rules](#business-rules)
- [How to Run](#how-to-run)
- [Automated Setup Script](#automated-setup-script)
- [Endpoints](#endpoints)
- [Tests](#tests)
- [Project Structure](#project-structure)
- [Technical Decisions](#technical-decisions)

---

## Overview

The **DeveloperStore API** is a complete RESTful API for managing a sales system, implementing:

- **Full CRUD** for Sales, Carts and Products
- **Quantity-based discount business rules** by number of items
- **Domain events** published on every relevant operation
- **JWT authentication** on all protected endpoints
- **Pagination and filtering** on all listing endpoints
- **~93% coverage** in the unit tests (240 tests)

---

## Technologies

### Backend
| Technology | Version | Usage |
|---|---|---|
| ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet) | 8.0 | Main framework |
| ![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=flat&logo=postgresql) | 13 | Relational database |
| ![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat&logo=dotnet) | 8.0 | ORM + Migrations |
| ![MediatR](https://img.shields.io/badge/MediatR-12.4-512BD4?style=flat&logo=dotnet) | 12.4 | CQRS / Mediator pattern |
| ![AutoMapper](https://img.shields.io/badge/AutoMapper-15.1.3-BE3C28?style=flat&logo=dotnet) | 15.1.3 | Object mapping |
| ![FluentValidation](https://img.shields.io/badge/FluentValidation-11.x-00B4AB?style=flat&logo=dotnet) | 11.x | Command validation |
| ![Serilog](https://img.shields.io/badge/Serilog-8.x-EF5B25?style=flat&logo=dotnet) | 8.x | Structured logging |
| ![Polly](https://img.shields.io/badge/Polly-8.x-FF6B35?style=flat&logo=dotnet) | 8.x | Retry policy for events |
| ![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?style=flat&logo=swagger&logoColor=black) | 6.x | API documentation |

### Tests
| Technology | Version | Usage |
|---|---|---|
| ![xUnit](https://img.shields.io/badge/xUnit-2.9-512BD4?style=flat&logo=dotnet) | 2.9 | Test framework (Unit, Integration, Functional) |
| ![Bogus](https://img.shields.io/badge/Bogus-35.6-512BD4?style=flat&logo=dotnet) | 35.6 | Fake data generation (Unit) |
| ![NSubstitute](https://img.shields.io/badge/NSubstitute-5.1-512BD4?style=flat&logo=dotnet) | 5.1 | Mocking (Unit) |
| ![FluentAssertions](https://img.shields.io/badge/FluentAssertions-6.12-00B4AB?style=flat&logo=dotnet) | 6.12 | Expressive assertions (all projects) |
| ![MockQueryable](https://img.shields.io/badge/MockQueryable-7.0-512BD4?style=flat&logo=dotnet) | 7.0 | Async IQueryable mocking (Unit) |
| ![Testcontainers](https://img.shields.io/badge/Testcontainers-3.10-2496ED?style=flat&logo=docker) | 3.10 | Real PostgreSQL in Integration and Functional tests |

### Infrastructure and DevOps
| Technology | Version | Usage |
|---|---|---|
| ![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat&logo=docker) | - | Containerization |
| ![MongoDB](https://img.shields.io/badge/MongoDB-8.0-47A248?style=flat&logo=mongodb&logoColor=white) | 8.0 | NoSQL (event store) |
| ![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?style=flat&logo=rabbitmq&logoColor=white) | 3.13 | Message broker (Rebus transport) |
| ![Rebus](https://img.shields.io/badge/Rebus-8.9-512BD4?style=flat&logo=dotnet&logoColor=white) | 8.9 | Service bus — publishing and consuming domain events |
| ![GitHub Actions](https://img.shields.io/badge/GitHub_Actions-CI/CD-2088FF?style=flat&logo=githubactions&logoColor=white) | - | Build, tests, coverage and Docker image push |

---

## Architecture

The project follows **Domain-Driven Design (DDD)** with **CQRS** via MediatR:

```
src/
├── Ambev.DeveloperEvaluation.Domain        # Entities, Events, Repositories, Validators
├── Ambev.DeveloperEvaluation.Application   # Commands, Queries, Handlers, Profiles
├── Ambev.DeveloperEvaluation.ORM           # DbContext, Repositories, Migrations, Events
├── Ambev.DeveloperEvaluation.IoC           # Dependency registration (DependencyResolver)
├── Ambev.DeveloperEvaluation.WebApi        # Controllers, Middleware, Program.cs
└── Ambev.DeveloperEvaluation.Common        # Security, Logging, HealthChecks, Validation

tests/
├── Ambev.DeveloperEvaluation.Unit          # Unit tests (~93% coverage, 240 tests)
├── Ambev.DeveloperEvaluation.Integration   # Integration tests with Testcontainers (40 tests)
└── Ambev.DeveloperEvaluation.Functional    # Functional tests
```

### Request flow

```
HTTP Request
     │
     ▼
Controller  (WebApi)
     │
     ▼
MediatR  →  Command / Query
     │
     ▼
Handler  (Application)
     │                         │  (write operations)
     ▼                         ▼
Repository  (ORM)        Domain Events
     │                         │
     ▼                         ▼
EF Core → PostgreSQL    EventPublisher
                    MongoEventPublisher (decorator)
                          → Rebus → RabbitMQ
```

### Patterns used

| Pattern | Implementation |
|---|---|
| **CQRS** | Separate Commands and Queries via MediatR |
| **Repository Pattern** | Data access abstraction |
| **Domain Events** | `SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, `ItemCancelledEvent` |
| **Specification Pattern** | `ActiveUserSpecification` |
| **External Identity** | Cart references Product by Id only (decoupled domains) |
| **Retry Pattern** | Polly with 3 attempts and exponential backoff on events |
| **GitFlow** | Feature branches from `develop`; `--no-ff` merges; types: `feature/`, `fix/`, `docs/`, `refactor/`, `chore/` |
| **Conventional Commits** | Semantic prefixes: `feat:`, `fix:`, `test:`, `refactor:`, `chore:`, `docs:` — auditable history compatible with changelog tools |

---

## Business Rules

### Discounts by quantity of identical items

| Quantity | Discount | Example ($100/unit) |
|---|---|---|
| Fewer than 4 items | No discount | 3 × $100 = **$300** |
| 4 to 9 items | **10%** | 5 × $100 × 0.90 = **$450** |
| 10 to 20 items | **20%** | 10 × $100 × 0.80 = **$800** |
| More than 20 items | ❌ Not allowed | — |

### Published domain events

| Event | When it is published |
|---|---|
| `SaleCreatedEvent` | When a sale is created |
| `SaleModifiedEvent` | When a sale is updated |
| `SaleCancelledEvent` | When a sale is deleted/cancelled |
| `ItemCancelledEvent` | When a sale item is cancelled |

---

## How to Run

### Prerequisites

![Docker](https://img.shields.io/badge/Docker_Desktop-Required-2496ED?style=flat&logo=docker)
![.NET](https://img.shields.io/badge/.NET_8_SDK-Optional-512BD4?style=flat&logo=dotnet)

### 1. Clone the repository

```bash
git clone https://github.com/raphaelfernandesribeiro/desafio-tecnico.git
cd desafio-tecnico/template/backend
# The source code lives in template/backend — pre-configured structure provided by the challenge
```

### 2. Start the containers

```bash
docker-compose up --build -d
```

> EF Core migrations are applied automatically on startup — no `dotnet ef database update` command is required.

| Service | URL |
|---|---|
| **API / Swagger UI** | http://localhost:8080 (Swagger at root `/`) |
| **Health Check** | http://localhost:8080/health |
| **PostgreSQL** | localhost:5432 |
| **MongoDB** | localhost:27017 |

### 3. Check the logs

```bash
docker logs ambev_developer_evaluation_webapi -f
```

### 4. Stop the containers

```bash
docker-compose down
```

### Environment variables

The project ships with working default values. To customize, copy the example file and edit it for your environment:

```bash
cp .env.example .env
# edit .env with your credentials
docker-compose up --build -d
```

| Variable | Default | Description |
|---|---|---|
| `POSTGRES_DB` | `developer_evaluation` | PostgreSQL database name |
| `POSTGRES_USER` | `developer` | PostgreSQL user |
| `POSTGRES_PASSWORD` | `ev@luAt10n` | PostgreSQL password |
| `MONGO_USER` | `developer` | MongoDB user |
| `MONGO_PASSWORD` | `ev@luAt10n` | MongoDB password |
| `JWT_SECRET_KEY` | `YourSuperSecretKey...` | JWT signing key (min. 32 chars) ⚠️ |
| `JWT_ISSUER` | `AmbevDeveloperEvaluation` | JWT token issuer |
| `JWT_AUDIENCE` | `AmbevDeveloperEvaluationUsers` | JWT token audience |
| `JWT_EXPIRY_MINUTES` | `60` | Token validity in minutes |

> ⚠️ **Production:** Replace `JWT_SECRET_KEY` with a strong, randomly generated key of at least 32 characters. The app validates the minimum length on startup and rejects short keys.

> To run without Docker, copy `appsettings.json`, set `DefaultConnection` to `Host=localhost` and run `dotnet run` in the `src/Ambev.DeveloperEvaluation.WebApi` folder.

---

## Automated Setup Script

For a one-shot, end-to-end run of the entire pipeline — from prerequisite checks through Docker teardown — the repository ships a PowerShell script at [`template/backend/scripts/setup.ps1`](template/backend/scripts/setup.ps1). It bootstraps the development environment from zero, runs every test suite, builds and starts the Docker stack, executes end-to-end API tests, and writes a per-step report under `setup-reports/<run-id>/`. At the end it prints a rich summary table.

### Requirements

| Tool | Required for |
|---|---|
| .NET SDK 8.0+ | Build, restore and tests |
| Docker Desktop | Docker build/up, integration & functional tests |
| Python 3.x | `ApiTest` step only (end-to-end API test) |
| `reportgenerator` | Coverage report — **auto-installed** by the Prerequisites step |

### Steps

The script runs 11 steps in order:

| # | Step | What it does |
|---|---|---|
| 1 | `Prerequisites` | Verify `dotnet`, `docker`, `python`, `reportgenerator` |
| 2 | `Restore` | `dotnet restore` |
| 3 | `Build` | `dotnet build --configuration Release` |
| 4 | `UnitTests` | `dotnet test` (unit suite only) |
| 5 | `Coverage` | Unit tests + HTML coverage via `reportgenerator` |
| 6 | `IntegrationTests` | `dotnet test` (integration suite, Testcontainers) |
| 7 | `FunctionalTests` | `dotnet test` (functional suite) |
| 8 | `DockerBuild` | `docker-compose build` |
| 9 | `DockerUp` | `docker-compose up -d` + wait for API |
| 10 | `ApiTest` | `python scripts/test_api.py` |
| 11 | `DockerDown` | `docker-compose down -v` |

### Usage

```powershell
cd template/backend/scripts

# Full run (all 11 steps), tearing down containers at the end
.\setup.ps1

# Full run, but keep containers running afterwards (skip DockerDown)
.\setup.ps1 -KeepContainers

# Run a single step — by number or by name
.\setup.ps1 -Step 4
.\setup.ps1 -Step Coverage

# Run from a given step to the end (-From)
.\setup.ps1 -Step 8 -From                       # DockerBuild → DockerDown
.\setup.ps1 -Step DockerBuild -From -KeepContainers

# Label this execution (report sub-folder); defaults to a timestamp
.\setup.ps1 -RunId my-run-label
```

| Parameter | Description |
|---|---|
| `-Step <name\|number>` | Run a single step. Accepts the step name (`UnitTests`) or number (`4`). Omit (or pass `All`) to run everything. |
| `-From` | Combined with `-Step`, runs from that step through the end instead of only that single step. |
| `-KeepContainers` | Skips the `DockerDown` step at the end of a full or `-From` run. |
| `-RunId <label>` | Optional label for the report sub-folder. Defaults to a timestamp. |

> **Exit codes:** `0` = all steps passed · `1` = one or more steps failed. Each step produces a dedicated report file under `setup-reports/<run-id>/`, plus a `00-summary.txt` overview.

> If your PowerShell execution policy blocks the script, run it with `powershell -ExecutionPolicy Bypass -File .\setup.ps1`.

---

## Endpoints

### 🔐 Auth

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/auth/login` | Authenticate user (field: `username`) | ❌ |

### 👤 Users

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/users` | Create user (nested schema: `name`, `address`) | ❌ |
| `GET` | `/api/users` | List users (paginated: `_page`, `_size`, `_order`) | ✅ |
| `GET` | `/api/users/{id}` | Get user by ID | ✅ |
| `PUT` | `/api/users/{id}` | Update profile (owner or Admin) | ✅ |
| `PATCH` | `/api/users/{id}/role` | Change user role/status | ✅ Admin |
| `DELETE` | `/api/users/{id}` | Delete user | ✅ |

### 📦 Products

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/products` | Create product | ✅ |
| `GET` | `/api/products` | List products (paginated) | ✅ |
| `GET` | `/api/products/{id}` | Get product by ID | ✅ |
| `GET` | `/api/products/categories` | List categories | ✅ |
| `GET` | `/api/products/category/{category}` | List by category | ✅ |
| `PUT` | `/api/products/{id}` | Update product | ✅ |
| `DELETE` | `/api/products/{id}` | Delete product | ✅ |

### 🛒 Carts

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/carts` | Create cart | ✅ |
| `GET` | `/api/carts` | List carts (paginated) | ✅ |
| `GET` | `/api/carts/{id}` | Get cart by ID | ✅ |
| `PUT` | `/api/carts/{id}` | Update cart | ✅ |
| `DELETE` | `/api/carts/{id}` | Delete cart | ✅ |

### 💰 Sales

| Method | Route | Description | Auth |
|---|---|---|---|
| `POST` | `/api/sales` | Create sale | ✅ |
| `GET` | `/api/sales` | List sales (paginated) | ✅ |
| `GET` | `/api/sales/{id}` | Get sale by ID | ✅ |
| `PUT` | `/api/sales/{id}` | Update sale | ✅ |
| `PATCH` | `/api/sales/{id}/items/{itemId}/cancel` | Cancel item | ✅ |
| `DELETE` | `/api/sales/{id}` | Cancel and delete sale | ✅ |

### Standard response format (success)

```json
{
  "data": { },
  "success": true,
  "message": "Operation successful"
}
```

### Error response format

```json
{
  "type": "ValidationError",
  "error": "Invalid request",
  "detail": "Username must be between 3 and 50 characters"
}
```

### Pagination and Ordering

Query parameters with the `_` prefix:
- `_page` (default: 1), `_size` (default: 10), `_order` (e.g. `username desc, email asc`)

```json
{
  "currentPage": 1,
  "totalPages": 3,
  "totalItems": 30,
  "data": []
}
```

### Filters

All filter parameters are optional and combinable with `&` (AND).

#### `GET /api/users`

| Parameter | Type | Description |
|---|---|---|
| `_page` | int | Page (default: 1) |
| `_size` | int | Items per page (default: 10) |
| `_order` | string | Ordering: `username asc`, `username desc`, `email asc`, `email desc` |

> No field filters — returns all users paginated.

#### `GET /api/products`

| Parameter | Type | Description |
|---|---|---|
| `_page` | int | Page (default: 1) |
| `_size` | int | Items per page (default: 10) |
| `_order` | string | `price asc`, `price desc`, `title asc`, `title desc` |
| `title` | string | Exact or partial match with `*` wildcard (`Samsung*`, `*phone`) |
| `category` | string | Exact match (case-insensitive): `electronics`, `jewelery`, etc. |
| `_minPrice` | decimal | Minimum price (inclusive) |
| `_maxPrice` | decimal | Maximum price (inclusive) |

```
GET /api/products?category=electronics&_minPrice=100&title=Samsung*
```

#### `GET /api/carts`

| Parameter | Type | Description |
|---|---|---|
| `_page` | int | Page (default: 1) |
| `_size` | int | Items per page (default: 10) |
| `_order` | string | Order by field (default: date desc) |
| `userId` | guid | Filter carts for a specific user |
| `_minDate` | datetime | Minimum cart date (inclusive) |
| `_maxDate` | datetime | Maximum cart date (inclusive) |

```
GET /api/carts?userId=3fa85f64-5717-4562-b3fc-2c963f66afa6&_minDate=2024-01-01
```

#### `GET /api/sales`

| Parameter | Type | Description |
|---|---|---|
| `_page` | int | Page (default: 1) |
| `_size` | int | Items per page (default: 10) |
| `_order` | string | `saledate asc`, `saledate desc`, `totalamount asc`, `totalamount desc`, `salenumber asc`, `salenumber desc` |
| `customerName` | string | Partial match (contains) on customer name |
| `isCancelled` | bool | `true` to list only cancelled sales, `false` for active ones |
| `_minDate` | datetime | Minimum sale date (inclusive) |
| `_maxDate` | datetime | Maximum sale date (inclusive) |

```
GET /api/sales?customerName=Test&isCancelled=false&_minDate=2024-01-01&_order=saledate desc
```

---

## Tests

> **Strategy:** **Integration** tests verify complete HTTP endpoints against a real database (Testcontainers) — they validate the API contract, authentication and business rules over HTTP. **Functional** tests run on an **isolated** PostgreSQL container (no shared state with integration) and focus exclusively on the domain's critical business rules (discount tiers, quantity limits, item cancellation), ensuring that code changes never break the core logic regardless of transport details.

### Unit Tests

```bash
cd template/backend
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Unit"
```

<div align="center">

![Coverage](https://img.shields.io/badge/Line_Coverage-93%25-brightgreen?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-240_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

### Generate coverage report

```powershell
# Install the tool (once)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Run on Windows
.\coverage-report.ps1

# Run on Linux/Mac
./coverage-report.sh
```

HTML report generated at `coverage-report/index.html`.

### Integration Tests

```bash
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Integration"
```

<div align="center">

![Tests](https://img.shields.io/badge/Integration_Tests-40_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

| Suite | Tests | Coverage |
|---|---|---|
| Auth / Users | 7 | Registration, authentication, JWT, 401 |
| Users (ownership + admin role) | 8 | PUT ownership check (200/403/401), PUT role change (owner→403, admin→200), PATCH role (200/403/401/404) |
| Products | 7 | Full CRUD, paginated listing, 401 |
| Sales | 8 | CRUD, discount rules (10%/20%), item cancellation, 401 |
| Carts | 7 | Full CRUD, paginated listing, 401 |
| E2E | 3 | Full flow: product → cart → sale with discount |

> ⚠️ Docker must be running — the tests automatically spin up a PostgreSQL container via **Testcontainers**.
> A single container is shared across all suites (`ICollectionFixture`) — fast startup (~15s).

### Functional Tests

```bash
dotnet test Ambev.DeveloperEvaluation.sln --filter "FullyQualifiedName~Functional"
```

<div align="center">

![Tests](https://img.shields.io/badge/Functional_Tests-3_passing-brightgreen?style=for-the-badge)
![Failures](https://img.shields.io/badge/Failures-0-brightgreen?style=for-the-badge)

</div>

| Scenario | What it verifies |
|---|---|
| Discount tiers across all brackets | No discount (3 items), 10% (5 items), 20% (15 items) — all in a single authenticated flow |
| Maximum quantity enforcement | A sale with 21 items is rejected with 400 (critical business rule) |
| Item cancellation state | Cancelling an item via PATCH marks the item as cancelled without cancelling the sale |

> ⚠️ Docker must be running — functional tests spin up a PostgreSQL container **isolated** from the integration container.

---

## Project Structure

```
template/backend/
├── src/
│   ├── Ambev.DeveloperEvaluation.Domain/
│   │   ├── Entities/          # Sale, SaleItem, Cart, CartItem, Product, User
│   │   ├── Events/            # SaleCreatedEvent, SaleModifiedEvent, etc.
│   │   ├── Repositories/      # ISaleRepository, ICartRepository, IProductRepository
│   │   ├── Validation/        # SaleValidator, CartValidator, ProductValidator
│   │   └── Specifications/    # ActiveUserSpecification
│   ├── Ambev.DeveloperEvaluation.Application/
│   │   ├── Sales/             # Create, Get, GetList, Update, Delete, CancelItem
│   │   ├── Carts/             # Create, Get, GetList, Update, Delete
│   │   ├── Products/          # Create, Get, GetList, Categories, Update, Delete
│   │   ├── Users/             # Create, Get, GetList, Update, PatchUserRole, Delete
│   │   └── Auth/              # AuthenticateUser (login by username)
│   ├── Ambev.DeveloperEvaluation.ORM/
│   │   ├── Repositories/      # SaleRepository, CartRepository, ProductRepository, UserRepository
│   │   ├── Services/          # RebusEventPublisher, LoggingEventPublisher (Polly retry), MongoEventPublisher (Decorator), MongoDbSettings, MongoDbExtensions
│   │   ├── Services/Messaging # SaleCreatedEventHandler, SaleModifiedEventHandler, SaleCancelledEventHandler, ItemCancelledEventHandler (Rebus IHandleMessages)
│   │   └── Migrations/        # InitialMigrations, AddSaleCartProduct, AddUserProfileFields
│   ├── Ambev.DeveloperEvaluation.IoC/
│   │   ├── DependencyResolver.cs          # Central registration point for all modules
│   │   ├── IModuleInitializer.cs          # Module contract
│   │   └── ModuleInitializers/            # ApplicationModuleInitializer, InfrastructureModuleInitializer, WebApiModuleInitializer
│   ├── Ambev.DeveloperEvaluation.Common/
│   │   ├── HealthChecks/      # HealthChecksExtension
│   │   ├── Logging/           # LoggingExtension (Serilog)
│   │   ├── Security/          # BCryptPasswordHasher, JwtTokenGenerator, IPasswordHasher, IJwtTokenGenerator
│   │   └── Validation/        # ValidationBehavior (MediatR pipeline), ValidationResult
│   └── Ambev.DeveloperEvaluation.WebApi/
│       ├── Features/          # Controllers, Requests, Responses, Profiles
│       └── Middleware/        # ValidationExceptionMiddleware
└── tests/
    ├── Ambev.DeveloperEvaluation.Unit/
    │   ├── Domain/            # Entity tests, Validator tests, Event tests
    │   ├── Application/       # Handler tests, Mapping tests
    │   └── Infrastructure/    # LoggingEventPublisher tests, MongoEventPublisher tests, RebusEventPublisher tests
    ├── Ambev.DeveloperEvaluation.Integration/
    │   ├── Fixtures/          # IntegrationTestFactory (Testcontainers), BaseIntegrationTest
    │   └── Features/          # Auth, Users, Products, Sales, Carts, E2E — 41 tests
    └── Ambev.DeveloperEvaluation.Functional/
        ├── Fixtures/          # FunctionalTestFactory (isolated container), BaseFunctionalTest
        └── Scenarios/         # SalesBusinessRulesTests — 3 business scenarios
```

---

## Backlog and Next Steps

What would be evolved with more time:

| Item | Description |
|---|---|
| ~~**Real message broker**~~ | ✅ Implemented: **Rebus 8.9 + RabbitMQ 3.13**. `RebusEventPublisher` is the inner publisher of the `MongoEventPublisher` decorator. In production it uses RabbitMQ (via `RabbitMq:ConnectionString`); in dev/tests it uses the InMemory transport automatically. Four `IHandleMessages<T>` handlers process the events in the same process. |
| ~~**Cart unit tests**~~ | ✅ Implemented: `CartHandlerTests` with 18 tests covering Create, Get, GetList (pagination, date filters, userId, ordering), Update and Delete |
| ~~**Functional tests**~~ | ✅ Implemented: `Ambev.DeveloperEvaluation.Functional` with 3 business scenarios (discount tiers, quantity limit, item cancellation) |
| **API versioning** | Route versioning (`/api/v1/`) to support evolution without breaking clients |
| **Rate limiting** | Per-IP/user throttling on public endpoints (auth, user creation) |
| ~~**CI/CD pipeline**~~ | ✅ Implemented: `.github/workflows/ci.yml` with build, unit tests (coverage), integration and functional tests (Testcontainers), coverage report as an artifact, and conditional Docker image build + push |

---

## Import into Postman

1. Open **Postman**
2. Click **Import**
3. Select `template/backend/docs/DeveloperStore.postman_collection.json`
4. Run in order: **Auth → Users → Products → Carts → Sales**

| Folder | Requests | Scripts |
|---|---|---|
| Auth | Create User, Authenticate | Login by `username`; saves `userId` and `token` automatically |
| Users | Get User, Delete User | Validates status 200 |
| Products | Create, Get All, Get by ID, Categories, By Category, Update, Delete | Saves `productId`; validates status and pagination |
| Carts | Create, Get All, Get by ID, Update, Delete | Dynamic date via Pre-request; saves `cartId` |
| Sales | Create, Get All, Get by ID, Update, Cancel Item, Delete | Unique `saleNumber` per run via `SALE-{timestamp}`; saves `saleId` and `saleItemId`; validates the 10% discount on Create |

> All 22 requests have test scripts with status assertions. The Pre-request Scripts in Cart and Sale Create/Update generate the date and saleNumber dynamically — the collection can be run multiple times without conflict.

---

## Technical Decisions

| Decision | Rationale |
|---|---|
| **AutoMapper 15.1.3** | Version with no known security vulnerabilities |
| **MockQueryable.NSubstitute** | Needed to test handlers that use EF's `CountAsync`/`ToListAsync` in unit tests |
| **Testcontainers** | Integration tests with a real database — higher reliability without repository mocks |
| **Polly retry on events** | Ensures resilience when publishing domain events (3 attempts, exponential backoff) |
| **External Identity on Cart** | Cart and Product are independent domains — intentional decoupling (DDD) |
| **SuppressModelStateInvalidFilter** | Ensures all validations go through the custom middleware |
| **Production in Docker** | Correct environment for containers — `Development` causes instability with DataProtection on Linux |
| **Explicit Kestrel on port 8080** | Ensures correct binding regardless of environment variables |
| **`[Authorize]` on controllers** | Products, Carts and Sales require JWT Bearer on every endpoint; Users exposes only `POST /api/users` as public (registration) |
| **`ICollectionFixture` in integration tests** | A single PostgreSQL container shared across all suites — eliminates the `BuildServiceProvider()` anti-pattern and reduces setup time from minutes to ~15s |
| **JWT Issuer + Audience validated** | `ValidateIssuer` and `ValidateAudience` enabled; the generated token includes `iss` and `aud` — tokens from other systems or with different keys are rejected |
| **JWT expiry read from config** | `JwtTokenGenerator` reads `Jwt:ExpiryMinutes` (default 60 min) — no hardcoded value; changeable via environment variable without a rebuild |
| **JWT secret minimum 32 bytes** | `AddJwtAuthentication` validates the minimum key length (HS256 / RFC 7518 requirement) on application startup |
| **Ownership check on `PUT /api/users/{id}`** | OWASP A01:2021 — any authenticated user could otherwise update another user's profile. The controller extracts the GUID from `ClaimTypes.NameIdentifier` and returns 403 if the caller is neither the owner nor an Admin |
| **`role` and `status` in the `PUT /api/users/{id}` body with a guard in the handler** | The spec requires these fields in the request body. The handler checks: if the caller is not an Admin and the submitted value differs from the current one, it throws `ForbiddenException` (403). Non-admins may omit or repeat the current value without restriction. Admins may change them freely. Spec compliance ✅ + security ✅ |
| **`PATCH /api/users/{id}/role` — admin only** | Dedicated endpoint with `[Authorize(Roles = "Admin")]` for administrators to manage any user's role and status without needing the profile endpoint |
| **Rebus as the message bus** | Framework required by `frameworks.md`. `RebusEventPublisher` implements `IEventPublisher` and delegates to `IBus.Publish`. The `MongoEventPublisher` decorator persists to MongoDB and forwards to Rebus, keeping clean architecture intact. Transport: RabbitMQ in production (configured via `RabbitMq:ConnectionString`); InMemory automatically in dev/tests (no container dependency). Four `IHandleMessages<T>` handlers structured-log the events — extensible to any future processing. |
| **GitFlow + Conventional Commits** | Explicit challenge criterion (spec overview, item #16). Feature branches created from `develop`, merged with `--no-ff` to preserve history, semantic prefixes (`feat:`, `fix:`, `test:`, `refactor:`, `chore:`, `docs:`). Auditable history readable by automated changelog tools. |
| **MongoDB as an active event store** | All domain events (`SaleCreatedEvent`, `SaleModifiedEvent`, `SaleCancelledEvent`, `ItemCancelledEvent`) are persisted to MongoDB via `MongoEventPublisher` (Decorator pattern over `LoggingEventPublisher`). The write is best-effort: a MongoDB failure does not block the flow — the event is still logged via Serilog + Polly. If `MongoDB:ConnectionString` is empty (local development without Docker), the decorator is suppressed and only `LoggingEventPublisher` is registered. |
| **`AsNoTracking()` on listing queries** | Applied to every repository's `GetAllQueryable()` (`SaleRepository`, `CartRepository`, `ProductRepository`, `UserRepository`). Read queries don't need EF Core change tracking — this reduces memory allocation and improves performance. **Not applied** to `GetByIdAsync()` because `DeleteAsync` calls it internally and needs tracked entities for a safe cascade delete. |

For a full, illustrated walkthrough of every architectural choice — with diagrams, code samples and an FAQ — see [`docs/DeveloperStore_Technical_Decisions.html`](template/backend/docs/DeveloperStore_Technical_Decisions.html) (also available as a PDF in the same folder).

---

<div align="center">

*Developed by **Raphael Ribeiro** — Developer Evaluation Technical Challenge 2026*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-13-4169E1?style=flat&logo=postgresql&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat&logo=docker&logoColor=white)
![Coverage](https://img.shields.io/badge/Coverage-93%25-brightgreen?style=flat)

</div>
