# Troubleshooting Guide

... (existing entries) ...

---

## 403 Forbidden when accessing Admin Pages

**Symptom**: Accessing `/admin/tenants` or `/admin/users` returns a 403 Forbidden error even after logging in.

**Cause**: The user account lacks the `passport_admin` role, or the OIDC client is not requesting/receiving the necessary scopes.

### Fix
1. **Verify Role**: Ensure the user has the `passport_admin` role in the `AspNetUserRoles` table.
2. **Configure Client**: In `QuestFlag.Passport.Services/Program.cs`, ensure the `passport-admin` client registration includes the `roles` scope:
   ```csharp
   manager.CreateAsync(new OpenIddictApplicationDescriptor {
       ClientId = "passport-admin",
       Permissions = {
           OpenIddictConstants.Permissions.Endpoints.Token,
           OpenIddictConstants.Permissions.GrantTypes.Password,
           OpenIddictConstants.Permissions.Scopes.Email,
           OpenIddictConstants.Permissions.Scopes.Profile,
           OpenIddictConstants.Permissions.Scopes.Roles // <--- Required
       }
   });
   ```

---

## EF Core Migrations — File Locking Error

**Symptom**: `The process cannot access the file ... because it is being used by another process` when running `dotnet ef database update`.

**Cause**: The application is running (either via IIS Express, Kestrel, or Docker) and holding a lock on the DLLs or the database file.

### Fix
1. Stop all running instances of the application.
2. If using Docker, run `docker compose down`.
3. Clean the solution: `dotnet clean`.
4. Run the migration command again.

---

## Kafka Topic Not Available

**Symptom**: `Subscribed topic not available: upload-completed: Broker: Unknown topic or partition`.

**Cause**: The topic has not been created in Kafka yet, and auto-topic creation is disabled or failing.

### Fix
1. Manually create the topic using Kafka CLI (if available) or ensure the producer publishes a message before the consumer starts.
2. In `docker-compose.override.yml`, ensure the listener configuration allows the broker to be reached correctly from both internal and external networks.

---

## Logout — 405 Method Not Allowed

**Symptom**: Clicking logout returns a 405 Method Not Allowed error.

**Cause**: The logout endpoint (typically `/auth/logout`) Expects a `POST` request for security reasons (CRSF protection), but the UI is sending a `GET` request.

### Fix
1. Update the logout button to use a form with `method="post"` or a JavaScript-triggered POST request.
2. In `AuthController`, ensure the `Logout` action is decorated with `[HttpPost]`.
