# QuestFlag Infrastructure Dependencies (.NET 10)

This document outlines the desired NuGet package dependencies and their latest stable versions, mapped to a Clean Architecture structure that includes a central API bootstraper (`ApiCore`) and a Generic Data Access Wrapper.

### 🏗️ 1. QuestFlag.Infrastructure.Domain
_Core domain models, entities, and repository interfaces. Contains no external infrastructure dependencies._
- *(No NuGet Packages)*

---

### 🧩 2. QuestFlag.Infrastructure.Application (BLL)
_Business rules, use cases, commands, queries, and DTO mappings._
- `MediatR` (14.0.0)
- `FluentValidation` (12.1.1)
- `AutoMapper` (16.0.0)

---

### 📦 3. QuestFlag.Infrastructure.Core (Generic Wrappers)
_Shared building blocks and generic wrappers around external infrastructure services, including the Base Repository and generic EF Core DB Context operations._
- `Microsoft.EntityFrameworkCore` (10.0.3)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (10.0.0)
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.3)
- `StackExchange.Redis` (2.11.8)
- `Microsoft.Extensions.Caching.StackExchangeRedis` (10.0.3)
- `Confluent.Kafka` (2.13.1)
- `Minio` (7.0.0)
- `Qdrant.Client` (1.17.0)
- `Neo4j.Driver` (6.0.0)
- `Polly` (8.6.5)

---

### 💾 4. QuestFlag.Infrastructure.Services (Persistence/Implementations)
_Specific data persistence logic and implementations of Domain interfaces. Utilizes the generic wrappers provided by `Core` to interact with the database without directly referencing EF Core packages._
- *(No direct EF Core/Infrastructure NuGet Packages required - inherits from Core)*

---

### ⚙️ 5. QuestFlag.Infrastructure.ApiCore (Base WebApp Library)
_Shared API Bootstrapper. This library registers all cross-cutting web concerns so individual API projects remain clean. It should provide extension methods (e.g., `AddQuestFlagApiDefaults()`)._

**Authentication & APIs:**
- `Microsoft.AspNetCore.Authentication.JwtBearer` (10.0.3)
- `Swashbuckle.AspNetCore` (10.1.4)

**Logging:**
- `Serilog.AspNetCore` (10.0.0)
- `Serilog.Sinks.Console` (6.1.1)
- `Serilog.Sinks.Grafana.Loki` (8.3.2)

**Observability & Instrumentation:**
- `OpenTelemetry` (1.15.0)
- `OpenTelemetry.Extensions.Hosting` (1.15.0)
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` (1.15.0)
- `OpenTelemetry.Instrumentation.AspNetCore` (1.15.0)
- `OpenTelemetry.Instrumentation.Http` (1.15.0)
- `OpenTelemetry.Instrumentation.Runtime` (1.15.0)
- `Npgsql.OpenTelemetry` (10.0.1)
- `OpenTelemetry.Instrumentation.StackExchangeRedis` (1.15.0-beta.1)
- `OpenTelemetry.Instrumentation.ConfluentKafka` (0.1.0-alpha.5)

**Health Checks:**
- `Microsoft.Extensions.Diagnostics.HealthChecks` (10.0.3)
- `AspNetCore.HealthChecks.NpgSql` (9.0.0)
- `AspNetCore.HealthChecks.Redis` (9.0.0)
- `AspNetCore.HealthChecks.Kafka` (9.0.0)

---

### 🚀 6. QuestFlag.Infrastructure.WebApp (Host / Executable)
_Startup project wiring and HTTP endpoints. This project's `Program.cs` should simply call the bootstraper from `ApiCore`._

**Database Tools (For CLI/Migrations only):**
- `Microsoft.EntityFrameworkCore.Design` (10.0.3)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.3)

---

### 💻 7. QuestFlag.Infrastructure.Client (Optional)
_UI client or API caller SDK._
- `Polly` (8.6.5) *(Optional for HTTP resilience)*
