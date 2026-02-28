# QuestFlag Architecture Dependencies (.NET 10)

This document maps NuGet package dependencies to each project in the Clean Architecture solution, organized by service set.

> **Solution file**: `src/QuestFlag.slnx`  
> **Framework**: `net10.0`  

---

## 🏗️ INFRASTRUCTURE SERVICE SET

### 1. QuestFlag.Infrastructure.Domain
_SDK: `Microsoft.NET.Sdk` | Core domain entities (`UploadRecord`, `UploadStatus`, `UserRole`) and repository interfaces._
- *(no NuGet packages)*

### 2. QuestFlag.Infrastructure.Application (BLL)
_SDK: `Microsoft.NET.Sdk` | Business rules, use cases (`UploadFileCommand`, `GetUploadsQuery`), validators._
- `MediatR` 12.x ✅ Installed
- `FluentValidation.DependencyInjectionExtensions` 11.x ✅ Installed

### 3. QuestFlag.Infrastructure.Core
_SDK: `Microsoft.NET.Sdk` | Shared implementations: `UploadRepository`, `AppDbContext`, `MinioStorageService`, `KafkaUploadEventPublisher`._
- `Microsoft.EntityFrameworkCore` 10.0.x ✅ Installed
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.x ✅ Installed
- `Confluent.Kafka` 2.x ✅ Installed
- `Minio` 7.x ✅ Installed
- `Google.Cloud.Storage.V1` 4.x ✅ Installed
- `Polly` 8.x ✅ Installed

### 4. QuestFlag.Infrastructure.Services (API Host)
_SDK: `Microsoft.NET.Sdk.Web` | Running Upload API (`/api/upload`)._
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.x ✅ Installed
- `Microsoft.EntityFrameworkCore.Design` 10.0.x ✅ Installed
- `Microsoft.EntityFrameworkCore.Tools` 10.0.x ✅ Installed
- `Swashbuckle.AspNetCore` 10.x ✅ Installed

### 5. QuestFlag.Infrastructure.Client (SDK)
_SDK: `Microsoft.NET.Sdk` | Client wrapper for Uploads API._
- *(no NuGet packages)*

---

## 🛂 PASSPORT SERVICE SET

### 1. QuestFlag.Passport.Domain
_SDK: `Microsoft.NET.Sdk` | Domain entities (`Tenant`, `ApplicationUser`)._
- *(no NuGet packages)*

### 2. QuestFlag.Passport.Application
_SDK: `Microsoft.NET.Sdk` | Business rules (`LoginCommand`, `GetTenantsQuery`)._
- `MediatR` 12.x ✅ Installed
- `FluentValidation.DependencyInjectionExtensions` 11.x ✅ Installed

### 3. QuestFlag.Passport.Core
_SDK: `Microsoft.NET.Sdk` | Implementations: `PassportDbContext`, Repositories._
- `Microsoft.EntityFrameworkCore` 10.0.x ✅ Installed
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.x ✅ Installed
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.x ✅ Installed
- `OpenIddict.EntityFrameworkCore` 5.x ✅ Installed

### 4. QuestFlag.Passport.Services (Auth Host)
_SDK: `Microsoft.NET.Sdk.Web` | Running Auth API (`/connect/token`, `/api/passport/tenants`)._
- `OpenIddict.AspNetCore` 5.x ✅ Installed
- `Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.x ✅ Installed
- `Microsoft.EntityFrameworkCore.Design` 10.0.x ✅ Installed
- `Microsoft.EntityFrameworkCore.Tools` 10.0.x ✅ Installed
- `Swashbuckle.AspNetCore` 10.x ✅ Installed

### 5. QuestFlag.Passport.Client (SDK)
_SDK: `Microsoft.NET.Sdk` | Client wrapper for Passport API._
- `Polly` 8.x ✅ Installed

---

## 🖥️ FRONTEND WEB APP (Blazor)

### 1. QuestFlag.Infrastructure.WebApp (Blazor Server Host)
_SDK: `Microsoft.NET.Sdk.Web` | Static asset provider and host. Uses TailwindCSS target._
- `Microsoft.AspNetCore.Components.WebAssembly.Server` 10.0.x ✅ Installed

### 2. QuestFlag.Infrastructure.WebApp.Client (WASM)
_SDK: `Microsoft.NET.Sdk.BlazorWebAssembly` | Client side views (`UploadsListPage`, `LoginPage`)._
- `Microsoft.AspNetCore.Components.WebAssembly` 10.0.x ✅ Installed
- `Microsoft.Extensions.Http` 10.0.x ✅ Installed
