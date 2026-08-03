# MechanicShop — Auto Repair Workshop Management System

[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4.svg)]()
[![Build & Test](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Tests](https://img.shields.io/badge/tests-688%20passed-brightgreen.svg)]()
[![Warnings](https://img.shields.io/badge/warnings-0-success.svg)]()

MechanicShop is an ASP.NET Core backend for a full auto repair workshop: customer and vehicle management, technician scheduling, work order lifecycles, billing and PDF invoicing, real-time notifications, and observability.

**Live Demo:** Coming soon — deployment link will be added after publishing.

---

## Why This Project

I wanted to build something closer to a real backend than a CRUD demo. So MechanicShop handles actual workshop problems — technician scheduling conflicts, work order state transitions, itemized invoicing — and it's backed by the same kind of infrastructure a real service needs: caching, rate limiting, health checks, structured logging, and distributed tracing.

---

## Architecture

![System Architecture](docs/images/architecture.svg)


The solution follows Clean Architecture with strict dependency direction: `Domain` has no outward dependencies, `Application` orchestrates use cases through MediatR against interfaces owned by the domain, and `Infrastructure`/`Api` are the only layers that know about EF Core, Redis, or HTTP.

### Project Structure

| Project | Layer | Responsibilities | Depends On |
|---|---|---|---|
| `MechanicShop.Domain` | Domain Core | Entities (`WorkOrder`, `Vehicle`, `Customer`), domain events, `Result<T>` structs | None |
| `MechanicShop.Contracts` | Contracts | Shared request/response DTOs | None |
| `MechanicShop.Application` | Application | MediatR commands/queries, validation & caching pipeline behaviors | `Domain`, `Contracts` |
| `MechanicShop.Infrastructure` | Infrastructure | EF Core `AppDbContext`, Identity, HybridCache, SignalR, background jobs | `Application`, `Domain` |
| `MechanicShop.Api` | Web API Host | Controllers, middleware, Scalar UI, health checks, OpenTelemetry | `Infrastructure`, `Application` |
| `MechanicShop.Client` | Blazor WASM UI | Standalone client, `CustomAuthenticationStateProvider`, SignalR client | `Contracts`, SignalR Client |
| `Tests/*` | Test Suite | 688 unit, subcutaneous, and integration tests via `Testcontainers.MsSql` | Target projects, Testcontainers |

---

## Features

| Capability | Technology / Mechanism |
|---|---|
| JWT Access Tokens | HMAC-SHA256 signed JSON Web Tokens |
| Secure Refresh Tokens | HttpOnly, `SameSite=Strict` secure cookies (XSS-protected) |
| Role Authorization | ASP.NET Core Identity (`Manager`, `Labor`) + custom requirements |
| CQRS | MediatR v12 commands & queries with `Result<T>` flow |
| Validation Pipeline | FluentValidation v11 pipeline behaviors |
| Domain Events | Dispatched via EF Core `SaveChangesAsync` interceptor |
| Auditable Entities | Automated `CreatedAtUtc`, `CreatedBy`, `LastModified` via interceptor |
| Real-Time Notifications | `WorkOrderHub` (SignalR) server & client push |
| Multi-Tier Caching | .NET 10 `HybridCache` (L1 in-memory) + Redis (L2 distributed) |
| Output Caching | `AddAppOutputCaching` + custom `AuthenticatedRequestCachingPolicy` |
| Rate Limiting | ASP.NET Core `RateLimiter` (fixed & sliding window policies) |
| API Versioning | `Asp.Versioning.Http` (URL path versioning, `/api/v1/workorders`) |
| API Documentation | Microsoft OpenAPI + Scalar UI |
| Global Exception Handling | `IExceptionHandler` producing RFC 7807 `ProblemDetails` |
| Health Checks | `/health/live` (liveness) & `/health/ready` (SQL/Redis/SMTP readiness) |
| Background Services | `OverdueBookingCleanupService` hosted service |
| Observability | OpenTelemetry (OTLP), Prometheus, Grafana, Jaeger, Seq |
| PDF Invoicing | Dynamic invoice generation via QuestPDF |
| Email Notifications | HTML emails via MailKit / MimeKit (SMTP) |
| Automated Testing | 688 tests across domain, application, subcutaneous, and integration layers |

### Key Engineering Decisions

| Decision | Rationale |
|---|---|
| CQRS without a repository layer | `DbContext` already implements Repository/Unit of Work; an extra abstraction over EF Core's LINQ surface would add indirection without benefit |
| `Result<T>` pattern | Explicit success/failure objects for expected domain and validation failures, instead of exceptions for control flow |
| HybridCache (L1 + L2) | In-memory L1 for speed, Redis L2 fallback to cut SQL round-trips while keeping cache consistency across instances |
| Domain events via EF interceptor | Entities record events (`AddDomainEvent`); an interceptor dispatches them through MediatR during `SaveChangesAsync`, decoupling side effects from domain state |
| Auditable entity interceptor | Populates `CreatedAtUtc`, `LastModifiedUtc`, `CreatedBy`, `LastModifiedBy` automatically, removing repetitive handler code |
| Testcontainers for integration tests | Runs real SQL Server in Docker during test execution to validate actual migrations and query behavior, no mocking |
| Central Package Management | Version pinning in `Directory.Packages.props` prevents dependency drift across all six solution projects |

---

## Technology Stack

**Backend**
ASP.NET Core Web API, .NET 10, C# 13

**Architecture**
Clean Architecture, CQRS, MediatR v12, FluentValidation v11

**Database**
Microsoft SQL Server 2022, EF Core 10

**Authentication**
ASP.NET Core Identity, JWT (HMAC-SHA256), secure HttpOnly refresh token cookies

**Caching**
.NET 10 `HybridCache`, Redis 7.4 Alpine (`StackExchange.Redis`), Output Caching

**Frontend**
Blazor WebAssembly (standalone, `net10.0`), `Microsoft.AspNetCore.SignalR.Client`

**Observability**
OpenTelemetry (OTLP), Prometheus, Grafana, Jaeger, Serilog, Seq 2024.3

**Documents & Messaging**
QuestPDF, MailKit / MimeKit (SMTP)

**Testing**
xUnit v2.9, FluentAssertions, NSubstitute, `Testcontainers.MsSql`, `WebApplicationFactory`

**Containerization**
Docker (non-root `app` user), Docker Compose v2

**CI/CD**
GitHub Actions

**API Tooling**
Microsoft OpenAPI, Scalar API Reference UI

---

## Security

- JWT access tokens (HMAC-SHA256) with refresh tokens stored in HttpOnly, `SameSite=Strict` cookies
- Role-based authorization (`Manager`, `Labor`) via ASP.NET Core Identity plus custom authorization requirements
- Rate limiting on the API surface (fixed and sliding window policies)
- Docker image runs as a non-root `app` user, with NuGet and Trivy vulnerability scans on every CI build

---

## Getting Started

### Run with Docker Compose

1. Copy the sample environment file:
   ```bash
   cp .env.example .env
   ```

2. Set the required secrets in `.env`:
   - `SA_PASSWORD`
   - `JWT_SECRET_KEY`
   - `MAIL_USERNAME`
   - `MAIL_PASSWORD`
   - `GF_SECURITY_ADMIN_PASSWORD`

3. Start the full stack:
   ```bash
   docker compose up --build -d
   ```

4. Verify the running services:

   | Service | URL |
   |---|---|
   | Scalar API UI | `http://localhost:5001/scalar/v1` |
   | Grafana Dashboard | `http://localhost:3000` (`admin` / password from `.env`) |
   | Seq Log Server | `http://localhost:8081` |
   | Jaeger Tracing UI | `http://localhost:16686` |
   | Health Probe | `http://localhost:5001/health/ready` |

### Seeded Accounts (local development only)

The app applies migrations and seeds these accounts on startup:

| Role | Email | Password | Access |
|---|---|---|---|
| Manager | `pm@localhost` | `pm@localhost` | Full administrative control, billing, labor assignment |
| Labor | `john.labor@localhost` | `john.labor@localhost` | Assigned work order updates, personal schedule view |

> These credentials are for local development only and must not be used, or reused, in any deployed environment.

---

## Testing

```
Total Test Suite: 688 Passed | 0 Failed | 0 Skipped
├── MechanicShop.Domain.UnitTests .......... 208 Passed (Domain logic & value objects)
├── MechanicShop.Application.UnitTests .....  30 Passed (Pipeline behaviors & mappers)
├── MechanicShop.Application.SubcutaneousTests 317 Passed (Use cases & MediatR handlers)
└── MechanicShop.Api.IntegrationTests ...... 133 Passed (Real SQL Server via Testcontainers)
```

All 133 API integration tests run against real, isolated SQL Server containers spun up dynamically via `Testcontainers.MsSql` — no mocked database layer.

---

## Continuous Integration

GitHub Actions ([`.github/workflows/ci.yml`](.github/workflows/ci.yml)) runs on every commit:

![CI Pipeline Execution](docs/images/ci.png)

1. **Format Gate** — `dotnet format --verify-no-changes`
2. **Security Audit** — NuGet package vulnerability scan
3. **Build & Test** — Release build, all 688 tests with coverage
4. **Docker Scan** — builds the container image and runs the Trivy vulnerability scanner

---

## Observability

| Service | Format | Local URL |
|---|---|---|
| Scalar API Documentation | OpenAPI 3.0 / Scalar UI | `http://localhost:5001/scalar/v1` |
| Grafana Dashboards | Metrics visualization | `http://localhost:3000` |
| Seq Log Server | Serilog ingestion | `http://localhost:8081` |
| Jaeger APM Tracing | OpenTelemetry OTLP | `http://localhost:16686` |
| Health Probes | JSON / UI | `http://localhost:5001/health/ready` |

---

## Database Design

### Entity Relationship Diagram

![Entity Relationship Diagram](docs/database/ERD.png)

### Relational Schema

![Relational Schema](docs/database/relational-schema.png)

Editable source files (`ERDiagram.erdplus`, `Relational Schema.erdplus`) are included under `docs/database/` for future changes.

---

## Screenshots

| Component | Preview |
|---|---|
| Login | ![Login](docs/images/login.png) |
| Dashboard | ![Dashboard](docs/images/dashboard.png) |
| Schedule View | ![Schedules](docs/images/schedules.png) |
| Schedule View (alt) | ![Schedules Alt](docs/images/schedules-2.png) |
| Work Orders | ![Work Orders](docs/images/work-orders.png) |
| Customers | ![Customers](docs/images/customers.png) |
| Services | ![Services](docs/images/services.png) |
| Vehicle Makes Management | ![Vehicle Makes](docs/images/vehicle-makes-management.png) |
| Employees Management | ![Employees](docs/images/employees-management.png) |
| Manager Settings | ![Manager Settings](docs/images/manager-settings.png) |
| Labor Settings | ![Labor Settings](docs/images/labor-settings.png) |
| PDF Invoice | ![Invoice PDF](docs/images/invoice-pdf.png) |
| Scalar API Docs | ![Scalar](docs/images/scalar.png) |
| Grafana | ![Grafana](docs/images/grafana.png) |
| Seq | ![Seq](docs/images/seq.png) |
| Jaeger | ![Jaeger](docs/images/jaeger.png) |
| Docker | ![Docker](docs/images/docker.png) |

Additional screenshots are available in [`docs/images/`](docs/images/).

---

## Author

**Moamen** — Backend Engineer (.NET / ASP.NET Core)
