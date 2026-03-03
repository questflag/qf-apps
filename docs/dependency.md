# QuestFlag Architecture Dependencies (.NET 10)

This document maps NuGet package dependencies to each project in the QuestFlag solution, organized by service set.

> **Solution file**: `QuestFlag.slnx`  
> **Framework**: `net10.0`
> **Orchestration**: `.NET Aspire 13.1.0`

---

## 🏗️ INFRASTRUCTURE SERVICE SET

### 1. QuestFlag.Infrastructure.Domain
_Core domain entities and repository interfaces._
- *(no NuGet packages)*

### 2. QuestFlag.Infrastructure.Core (DAL)
_Infrastructure-specific implementations (Storage, Messaging)._
- `Microsoft.EntityFrameworkCore` 10.x ✅
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x ✅
- `Confluent.Kafka` 2.x ✅
- `Minio` 7.x ✅
- `Google.Cloud.Storage.V1` 4.x ✅
- `Polly` 8.x ✅

### 4. QuestFlag.Infrastructure.Services (API Host)
_Upload API and shared Infrastructure services._
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.x ✅
- `Microsoft.AspNetCore.OpenApi` 10.x ✅
- `OpenIddict.Validation.AspNetCore` 7.x ✅
- `Swashbuckle.AspNetCore` 10.x ✅

### 5. QuestFlag.Infrastructure.ApiCore
_Shared API logic and middleware._
- `Microsoft.AspNetCore.Http.Abstractions` 10.x ✅

---

## 🛂 PASSPORT SERVICE SET

### 1. QuestFlag.Passport.Domain
_Identity and Tenant domain entities._
- *(no NuGet packages)*

### 2. QuestFlag.Passport.Application
_Passport business rules (Login, Registration, Tenant Management)._
- `MediatR` 14.x ✅
- `FluentValidation.DependencyInjectionExtensions` 12.x ✅

### 3. QuestFlag.Passport.Core
_Identity and Passport data implementations._
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.x ✅
- `Microsoft.EntityFrameworkCore` 10.x ✅
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.x ✅
- `OpenIddict.EntityFrameworkCore` 7.x ✅

### 4. QuestFlag.Passport.Services (Auth Host)
_OIDC and Identity Service API._
- `OpenIddict.AspNetCore` 7.x ✅
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.x ✅
- `Swashbuckle.AspNetCore` 10.x ✅

---

## 🖥️ FRONTEND APPLICATIONS (Blazor InteractiveAuto)

### 1. WebApps (Infrastructure, Passport, Admin)
_Blazor Server Host projects._
- `Microsoft.AspNetCore.Components.WebAssembly.Server` 10.x ✅

### 2. WebApp.Client Projects
_Blazor WebAssembly client-side views._
- `Microsoft.AspNetCore.Components.WebAssembly` 10.x ✅
- `Microsoft.Extensions.Http` 10.x ✅

---

## 🚀 DEPLOYMENT & ORCHESTRATION

### 1. QuestFlag.AppHost
_Aspire Orchestration logic._
- `Aspire.AppHost.Sdk` 13.x ✅

### 2. QuestFlag.ServiceDefaults
_Common configuration for OTel, Health Checks, and Discovery._
- `Microsoft.Extensions.Http.Resilience` 10.x ✅
- `Microsoft.Extensions.ServiceDiscovery` 10.x ✅
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.x ✅
- `OpenTelemetry.Extensions.Hosting` 1.x ✅
- `OpenTelemetry.Instrumentation.AspNetCore` 1.x ✅
- `OpenTelemetry.Instrumentation.Http` 1.x ✅
- `OpenTelemetry.Instrumentation.Runtime` 1.x ✅
