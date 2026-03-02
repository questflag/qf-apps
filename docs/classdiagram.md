# Class Diagram

This diagram show the relationships between core domain entities in the QuestFlag and Passport solutions.

```mermaid
classDiagram
    class Tenant {
        +Guid Id
        +string Name
        +string Slug
        +bool IsActive
        +DateTime CreatedAtUtc
        +string? CustomDomain
        +string? SubdomainSlug
    }

    class ApplicationUser {
        +Guid TenantId
        +string DisplayName
        +bool IsActive
        +DateTime CreatedAtUtc
        +DateTime? LastLogoutAtUtc
        +ICollection~UserAgent~ UserAgents
    }

    class TrustedDevice {
        +Guid Id
        +Guid UserId
        +string DeviceTokenHash
        +string DeviceName
        +string IpAddress
        +DateTime TrustedAtUtc
        +DateTime ExpiresAtUtc
        +bool IsRevoked
    }

    class UserAgent {
        +Guid UserId
        +string ClientId
    }

    class UploadRecord {
        +Guid Id
        +Guid TenantId
        +Guid UserId
        +string OriginalFileName
        +string StoredFileName
        +string BucketName
        +string ObjectKey
        +string TaskName
        +string Category
        +long SizeInBytes
        +string[] Tags
        +Dictionary~string,string~ ExtraData
        +UploadStatus Status
        +string? ErrorMessage
        +int RetryCount
        +DateTime CreatedAtUtc
        +DateTime? CompletedAtUtc
        +bool IsDeleted
    }

    Tenant "1" -- "0..*" ApplicationUser : possesses
    ApplicationUser "1" -- "0..*" TrustedDevice : uses
    ApplicationUser "1" -- "0..*" UserAgent : associated with
    Tenant "1" -- "0..*" UploadRecord : owns
    ApplicationUser "1" -- "0..*" UploadRecord : performs
```

## Description of Relationships

- **Tenant to ApplicationUser**: A multi-tenant system where each user belongs to exactly one tenant.
- **ApplicationUser to TrustedDevice**: A user can have multiple trusted devices for bypassing 2FA.
- **ApplicationUser to UserAgent**: Users are associated with specific client agents for authorization.
- **Tenant/User to UploadRecord**: Each file upload is associated with a specific user and their tenant for isolation and billing.
