# QuestFlag Infrastructure Dependencies (.NET 10)

This document maps NuGet package dependencies to each project in the Clean Architecture solution.
It distinguishes between **currently installed** packages (present in `.csproj`) and **planned** packages (intended for future addition as features are built out).

> **Solution file**: `src/QuestFlag.slnx`  
> **Framework**: `net10.0` across all projects

---

## Solution Layout

```
src/
├── QuestFlag.Infrastructure.Domain/           # Domain models & repository interfaces
├── QuestFlag.Infrastructure.Application/      # BLL: use cases, commands, queries, DTOs
├── QuestFlag.Infrastructure.Core/             # Generic wrappers (repos, EF, caching, etc.)
├── QuestFlag.Infrastructure.Services/         # Running API host (Microsoft.NET.Sdk.Web)
├── QuestFlag.Infrastructure.ApiCore/          # Shared API bootstrapper library
├── QuestFlag.Infrastructure.Client/           # Shared client SDK / API caller
└── QuestFlag.Infrastructure.WebApp/
    ├── QuestFlag.Infrastructure.WebApp/        # Blazor hybrid host (Sdk.Web)
    └── QuestFlag.Infrastructure.WebApp.Client/ # Blazor WASM client (Sdk.BlazorWebAssembly)
```

### Project Reference Graph

```
Domain ──────────────────────────────────────────────► Core
Application ─────────────────────────────────────────► (none yet)
Services (API Host) ──────────────────────────────────► Application, Domain
WebApp (Blazor Host) ────────────────────────────────► Client, WebApp.Client
WebApp.Client (WASM) ─────────────────────────────────► (none yet)
```

---

## 🏗️ 1. QuestFlag.Infrastructure.Domain

_SDK: `Microsoft.NET.Sdk` | Core domain models, entities, repository interfaces._

| Package | Version | Status |
|---------|---------|--------|
| *(no NuGet packages)* | — | — |

> **Project reference**: → `QuestFlag.Infrastructure.Core`

---

## 🧩 2. QuestFlag.Infrastructure.Application (BLL)

_SDK: `Microsoft.NET.Sdk` | Business rules, use cases, commands, queries, and DTO mappings._

| Package | Version | Status |
|---------|---------|--------|
| `MediatR` | 14.x | 📋 Planned |
| `FluentValidation` | 12.x | 📋 Planned |
| `AutoMapper` | 16.x | 📋 Planned |

---

## 📦 3. QuestFlag.Infrastructure.Core (Generic Wrappers)

_SDK: `Microsoft.NET.Sdk` | Shared building blocks: base repository, generic EF Core context, caching, messaging._

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.EntityFrameworkCore` | 10.0.x | 📋 Planned |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.x | 📋 Planned |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | 10.0.x | 📋 Planned |
| `StackExchange.Redis` | 2.x | 📋 Planned |
| `Microsoft.Extensions.Caching.StackExchangeRedis` | 10.0.x | 📋 Planned |
| `Confluent.Kafka` | 2.x | 📋 Planned |
| `Minio` | 7.x | 📋 Planned |
| `Qdrant.Client` | 1.x | 📋 Planned |
| `Neo4j.Driver` | 6.x | 📋 Planned |
| `Polly` | 8.x | 📋 Planned |

---

## 💾 4. QuestFlag.Infrastructure.Services (API Host)

_SDK: `Microsoft.NET.Sdk.Web` | The running API host. Wires up DI, middleware, and HTTP endpoints. References `Application` and `Domain`._

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.AspNetCore.OpenApi` | 10.0.3 | ✅ Installed |
| `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` | 1.23.0 | ✅ Installed |

> **Project references**: → `QuestFlag.Infrastructure.Application`, `QuestFlag.Infrastructure.Domain`

> **Note**: EF Core design/tooling packages belong here (as dev/CLI tools), not in `Core`.

---

## ⚙️ 5. QuestFlag.Infrastructure.ApiCore (Base WebApp Library)

_SDK: `Microsoft.NET.Sdk` | Shared API bootstrapper — registers cross-cutting web concerns via extension methods (e.g. `AddQuestFlagApiDefaults()`)._

**Authentication & APIs:**

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 10.0.x | 📋 Planned |
| `Swashbuckle.AspNetCore` | 10.x | 📋 Planned |

**Logging:**

| Package | Version | Status |
|---------|---------|--------|
| `Serilog.AspNetCore` | 10.x | 📋 Planned |
| `Serilog.Sinks.Console` | 6.x | 📋 Planned |
| `Serilog.Sinks.Grafana.Loki` | 8.x | 📋 Planned |

**Observability & Instrumentation:**

| Package | Version | Status |
|---------|---------|--------|
| `OpenTelemetry` | 1.x | 📋 Planned |
| `OpenTelemetry.Extensions.Hosting` | 1.x | 📋 Planned |
| `OpenTelemetry.Exporter.OpenTelemetryProtocol` | 1.x | 📋 Planned |
| `OpenTelemetry.Instrumentation.AspNetCore` | 1.x | 📋 Planned |
| `OpenTelemetry.Instrumentation.Http` | 1.x | 📋 Planned |
| `OpenTelemetry.Instrumentation.Runtime` | 1.x | 📋 Planned |
| `Npgsql.OpenTelemetry` | 10.0.x | 📋 Planned |
| `OpenTelemetry.Instrumentation.StackExchangeRedis` | 1.x-beta | 📋 Planned |
| `OpenTelemetry.Instrumentation.ConfluentKafka` | 0.1.x-alpha | 📋 Planned |

**Health Checks:**

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.Extensions.Diagnostics.HealthChecks` | 10.0.x | 📋 Planned |
| `AspNetCore.HealthChecks.NpgSql` | 9.x | 📋 Planned |
| `AspNetCore.HealthChecks.Redis` | 9.x | 📋 Planned |
| `AspNetCore.HealthChecks.Kafka` | 9.x | 📋 Planned |

---

## 💻 6. QuestFlag.Infrastructure.Client (Client SDK)

_SDK: `Microsoft.NET.Sdk` | Shared client SDK or API caller. Referenced by the Blazor WebApp host._

| Package | Version | Status |
|---------|---------|--------|
| `Polly` | 8.x | 📋 Planned (HTTP resilience) |

> **Referenced by**: `QuestFlag.Infrastructure.WebApp`

---

## 🌐 7. QuestFlag.Infrastructure.WebApp (Blazor Host)

_SDK: `Microsoft.NET.Sdk.Web` | Blazor hybrid host — serves both SSR and WebAssembly client assets. `Program.cs` is minimal._

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.AspNetCore.Components.WebAssembly.Server` | 10.0.3 | ✅ Installed |

> **Project references**: → `QuestFlag.Infrastructure.Client`, `QuestFlag.Infrastructure.WebApp.Client`

---

## 🖥️ 8. QuestFlag.Infrastructure.WebApp.Client (Blazor WASM)

_SDK: `Microsoft.NET.Sdk.BlazorWebAssembly` | Client-side Blazor WebAssembly application running in the browser._

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.AspNetCore.Components.WebAssembly` | 10.0.3 | ✅ Installed |

---

## Database Tooling (CLI / Migrations)

The following packages should be added to `QuestFlag.Infrastructure.Services` (or a dedicated `Migrations` project) as **dev/CLI-only** tools when EF Core migrations are needed:

| Package | Version | Status |
|---------|---------|--------|
| `Microsoft.EntityFrameworkCore.Design` | 10.0.x | 📋 Planned |
| `Microsoft.EntityFrameworkCore.Tools` | 10.0.x | 📋 Planned |

---

## Legend

| Icon | Meaning |
|------|---------|
| ✅ Installed | Present in `.csproj` — currently active |
| 📋 Planned | Intended/designed package — not yet added to `.csproj` |
