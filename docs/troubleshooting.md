# Troubleshooting Guide

## HTTPS Dev Certificate Error in Visual Studio

**Error**: `Unexpected error saving password for the certificate in the user secrets`

This occurs when the ASP.NET Core HTTPS development certificate is in a bad state when running Docker containers from Visual Studio.

### Fix

**Step 1 — Clean the existing broken certificate:**

```powershell
dotnet dev-certs https --clean
```

**Step 2 — Generate and trust a fresh one:**

```powershell
dotnet dev-certs https --trust
```

> Accept the Windows security prompt to add the certificate to the Trusted Root Authorities store.

**Step 3 — Restart Visual Studio and Docker containers:**

```bash
docker compose down
docker compose up -d
```

**Verify the certificate is valid:**

```powershell
dotnet dev-certs https --check --trust
# Expected: A valid certificate was found.
```

---

## Tempo Configuration Error (file not found)

**Error**: `tempo: failed to load config: /etc/tempo.yaml: no such file or directory`

The Tempo image expects a `.yaml` extension, but the config file was named `.yml`.

### Fix

1. Rename `Configs/tempo.yml` → `Configs/tempo.yaml`
2. Update `docker-compose.override.yml`:
   ```yaml
   tempo:
     command: [ "-config.file=/etc/tempo.yaml" ]
     volumes:
       - ./Configs/tempo.yaml:/etc/tempo.yaml
   ```

---

## Tempo — Invalid `compactor` Field

**Error**: `failed parsing config: field compactor not found in type app.Config`

The top-level `compactor` key was removed in newer Tempo. Block retention now lives inside `storage.trace.block`.

### Fix in `Configs/tempo.yaml`

If you encounter `field compactor not found` or `field retention not found`, simpler is better. Most modern Tempo versions use sane defaults, and specific storage fields are often version-sensitive.

1. Ensure the extension is `.yaml`.
2. Remove any problematic `compactor` or `block_retention` blocks if they cause startup failures.

---

## OTel Collector — Unknown Exporter Type `loki`

**Error**: `'exporters' unknown type: "loki"`

The `otel/opentelemetry-collector-contrib` image does not include a standalone `loki` exporter. Loki (since v2.9) accepts OTLP natively via its `/otlp` endpoint.

### Fix in `Configs/otel-collector.yml`

```yaml
exporters:
  # Remove this:
  # loki:
  #   endpoint: http://loki:3100/loki/api/v1/push

  # Use this instead:
  otlphttp/loki:
    endpoint: http://loki:3100/otlp

service:
  pipelines:
    logs:
      exporters: [otlphttp/loki]
```

---

## Loki — Schema v11 + boltdb-shipper Incompatible with OTLP

**Error**: `schema v13 is required to store Structured Metadata... your schema version is v11`

Loki's native OTLP ingestion requires schema v13 and `tsdb` index type.

### Fix in `Configs/loki.yml`

Add a new schema entry (keep the old one for existing data):

```yaml
schema_config:
  configs:
    - from: 2020-10-24        # old entry — keep for existing data
      store: boltdb-shipper
      schema: v11
      ...
    - from: 2026-03-01        # new entry — required for OTLP
      store: tsdb
      object_store: filesystem
      schema: v13
      index:
        prefix: index_
        period: 24h

limits_config:
  allow_structured_metadata: true
```

---

## Kafka Restart Loop (KRaft mode)

**Symptom**: Kafka container starts and immediately restarts in KRaft mode.

**Cause**: Without a fixed `KAFKA_KRAFT_CLUSTER_ID`, Bitnami Kafka generates a new cluster ID every container start. This conflicts with the existing ID persisted in the `kafka_data` volume.

### Fix

Add a stable cluster ID to `docker-compose.override.yml`:

```yaml
kafka:
  environment:
    - KAFKA_KRAFT_CLUSTER_ID=L697XvT_Scy_Y_Z7Q_L7_A
```

> If Kafka still restarts after this fix, delete the stale volume and recreate it:
> ```bash
> docker volume rm deployment_kafka_data
> docker compose up -d kafka
> ```
