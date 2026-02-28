# Entity Relationship (ER) Diagram

This diagram show the database schema relationships for the QuestFlag solution.

```mermaid
erDiagram
    TENANT ||--o{ APPLICATION_USER : "belongs to"
    APPLICATION_USER ||--o{ TRUSTED_DEVICE : "trusts"
    TENANT ||--o{ UPLOAD_RECORD : "owns"
    APPLICATION_USER ||--o{ UPLOAD_RECORD : "uploads"

    TENANT {
        Guid id PK
        string name
        string slug
        bool is_active
        datetime created_at_utc
        string custom_domain
        string subdomain_slug
    }

    APPLICATION_USER {
        Guid id PK
        Guid tenant_id FK
        string display_name
        bool is_active
        datetime created_at_utc
        datetime last_logout_at_utc
        string email
        string phone_number
    }

    TRUSTED_DEVICE {
        Guid id PK
        Guid user_id FK
        string device_token_hash
        string device_name
        string ip_address
        datetime trusted_at_utc
        datetime expires_at_utc
        bool is_revoked
    }

    UPLOAD_RECORD {
        Guid id PK
        Guid tenant_id FK
        Guid user_id FK
        string original_file_name
        string stored_file_name
        string bucket_name
        string object_key
        string task_name
        string category
        long size_in_bytes
        int status
        datetime created_at_utc
        datetime completed_at_utc
        bool is_deleted
    }
```

## Schema Overview

- **TENANT**: The root of multi-tenancy. All data is partitioned by `tenant_id`.
- **APPLICATION_USER**: Extends the default Identity user, linked to a specific tenant.
- **TRUSTED_DEVICE**: Security feature for 2FA bypass.
- **UPLOAD_RECORD**: Tracks the lifecycle of uploaded files across the system.
