# QuestFlag Communication Platform -- Architecture & Requirements (v15)

**Version:** 15.0  
**Date:** 2026-03-01  

## Document Overview
This document provides a comprehensive and detailed architectural breakdown of the QuestFlag Communication Module. It includes elaborate state diagrams, sequence diagrams, activity diagrams, block diagrams, and configuration tables to define the system's behavior and structure.

## Table of Contents
## Table of Contents
1. [Core Communication Flow](#1-core-communication-flow)
2. [Conversation Management](#2-conversation-management)
3. [Real-Time Media Pipelines (Voice & Video)](#3-real-time-media-pipelines-voice--video)
4. [Infrastructure & Buffering](#4-infrastructure--buffering)
5. [Database Architecture & Schemas](#5-database-architecture--schemas)
6. [Project Folder Structure & Dependencies](#6-project-folder-structure--dependencies)

---

## 1. Core Communication Flow

### 1.1 Generic Communication Lifecycle (State Diagram)
**Overview:** Tracks the lifecycle of an outbound generic message (SMS, Email, Push) from creation to final delivery status.

```mermaid
stateDiagram-v2
    [*] --> CREATED: Message initiated
    CREATED --> VALIDATED: Validation successful
    CREATED --> CANCELLED: Validation failed or aborted
    VALIDATED --> QUEUED: Enqueued in message broker
    VALIDATED --> CANCELLED: Processing aborted
    QUEUED --> SENDING: Picked by worker
    SENDING --> SENT: Accepted by provider
    SENDING --> RETRYING: Transient failure
    RETRYING --> SENDING: Retry attempt triggered
    RETRYING --> FAILED: Max retries met
    FAILED --> DEAD_LETTERED: Moved to DLQ
    SENT --> DELIVERED: Webhook delivery confirmation
    DELIVERED --> READ: Read receipt webbook
    READ --> [*]
    DEAD_LETTERED --> [*]
    CANCELLED --> [*]
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `COMM_MAX_RETRIES` | Integer | `3` | Maximum retry attempts for transient sending failures. |
| `COMM_RETRY_DELAY_MS`| Integer | `5000` | Delay between retry attempts. |
| `COMM_ENABLE_DLQ` | Boolean | `true` | Enables moving failed messages to a Dead Letter Queue. |

### 1.2 Generic Communication Flow (Sequence Diagram)
**Overview:** Describes the step-by-step procedure of an agent initiating a communication request, being validated, queued, and dispatched via external providers.

```mermaid
sequenceDiagram
    participant Agent as App/Agent
    participant Comm as Comm Service
    participant Passport as Auth/Passport
    participant Resolver as Provider Resolver
    participant Queue as Kafka/RabbitMQ
    participant Worker as Delivery Worker
    participant DB as MongoDB
    participant Infra as Provider Sandbox

    Agent->>Comm: Send Message (Payload)
    activate Comm
    Comm->>Passport: Validate Sender Auth
    Passport-->>Comm: Authorized
    Comm->>Resolver: Resolve Best Provider Config
    Resolver-->>Comm: Provider Chosen (Twilio/SendGrid)
    Comm->>Queue: Enqueue Message Task
    Comm-->>Agent: 202 Accepted (Transaction ID)
    deactivate Comm

    Queue->>Worker: Consume Task
    activate Worker
    Worker->>DB: Update State (QUEUED -> SENDING)
    Worker->>Infra: Dispatch API Call to Provider
    Infra-->>Worker: Normalized Response (Success/Fail)
    Worker->>DB: Update State (SENT / FAILED)
    deactivate Worker
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `RESOLVER_STRATEGY`| String | `COST` | Strategy for provider routing (COST, SPEED, RELIABILITY). |
| `QUEUE_MAX_BATCH` | Integer | `100` | Number of tasks pulled per worker iteration. |
| `AUTH_CACHE_TTL` | Integer | `3600` | Passport token cache expiry in seconds. |

### 1.3 Generic Communication Processing (Activity Diagram)
**Overview:** A top-level algorithmic flow for processing outbound communication logic within the orchestration layer.

```mermaid
flowchart TD
    Start((Start)) --> Validate[Validate Payload & Schema]
    Validate --> |Invalid| ErrorCancel[Cancel Transaction]
    Validate --> |Valid| ResolveProvider[Resolve Optimal Provider]
    ResolveProvider --> CreateTransaction[Persist Transaction to DB]
    CreateTransaction --> QueueMessage[Push Task to Message Broker]
    
    QueueMessage --> WorkerProcessing{Worker Dispatches Call}
    WorkerProcessing -->|Provider Error| Failure[Log Failure]
    WorkerProcessing -->|Provider OK| Success[Log Success]
    
    Failure --> RetryCheck{Retry Limit Reached?}
    RetryCheck -->|No| QueueMessage
    RetryCheck -->|Yes| DLQ[Move to DLQ]
    
    Success --> PersistState[Update State to SENT/DELVERED]
    PersistState --> End((End))
    DLQ --> End
    ErrorCancel --> End
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `VALIDATION_SCHEMA`| String | `v2.0` | Schema version to apply. |
| `DLQ_TOPIC` | String | `qf-dlq` | Kafka topic or AMQP queue name for dead letters. |

### 1.4 Generic Communication Pipeline (Block Diagram)
**Overview:** Illustrates the microservices components and logical network segregation involved in transmitting messages asynchronously.

```mermaid
flowchart LR
    subgraph Client Layer
        A[App/Agent Interface]
    end
    
    subgraph Core Services
        B[Comm Orchestrator]
        C[Passport Auth]
        D[Provider Resolver]
        E[Transaction Manager]
    end
    
    subgraph Message Broker
        F[(Event Bus / Queue)]
    end
    
    subgraph Worker Nodes
        G[Delivery Worker Pool]
        H[Provider SDK wrapper]
    end
    
    subgraph Infrastructure
        I((External APIs))
        J[(MongoDB Cluster)]
    end

    A --> B
    B <--> C
    B <--> D
    B --> E
    E --> F
    F --> G
    G --> H
    H --> I
    G --> J
    E --> J
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `WORKER_POOL_SIZE` | Integer | `10` | Number of instances processing the outbound queues. |
| `BROKER_URL` | String | `kafka:9092`| Broker endpoints configuration. |

---

## 2. Conversation Management

### 2.1 Conversation Lifecycle (State Diagram)
**Overview:** Conversations represent a bidirectional exchange of messages. This lifecycle manages the active status, waiting periods, and archival of conversational thread segments.

```mermaid
stateDiagram-v2
    [*] --> CONVERSATION_CREATED: Initial contact
    CONVERSATION_CREATED --> ACTIVE: Participant joins
    ACTIVE --> WAITING_FOR_REPLY: User replied, waiting AI/Agent
    WAITING_FOR_REPLY --> ACTIVE: AI/Agent responds
    ACTIVE --> ENDING: Explicit End / 30m Sliding Timeout
    WAITING_FOR_REPLY --> ENDING: 30m Sliding Timeout
    ENDING --> SUMMARIZING: Trigger LLM Summary
    SUMMARIZING --> CLOSED: Single Conversation File Complete
    CLOSED --> ARCHIVED: Move to Archived Collection
    ARCHIVED --> [*]
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `CONV_TIMEOUT_MINS` | Integer | `30` | Sliding window of inactivity before marking as ENDING. |
| `CONV_ENABLE_LLM_SUMMARY`| Boolean | `true` | Extract facts, numbers, and summarize thread on closure. |
| `DB_COLL_ACTIVE_CONV` | String | `threads_active` | MongoDB collection for in-progress threads. |
| `DB_COLL_ARCHIVED_CONV` | String | `threads_archived`| MongoDB collection for single compiled threads. |

### 2.2 Conversation Flow (Sequence Diagram)
**Overview:** Handles inbound messages webhook from a provider, maintaining contextual threads and triggering automated responses.

```mermaid
sequenceDiagram
    participant User as End User
    participant Hook as Webhook Gateway
    participant Comm as Comm Service
    participant DB as MongoDB
    participant Queue as Event Queue

    User->>Hook: Sends SMS/Reply
    Hook->>Comm: Process Inbound Payload
    activate Comm
    Comm->>DB: Identify Existing Conversation
    alt exists
        Comm->>DB: Append Message to Thread
    else new
        Comm->>DB: Create Conversation & Append
    end
    Comm->>Queue: Emit CONVERSATION_UPDATED Event
    Queue-->>Comm: Queue Accepted
    Comm->>Queue: Trigger Auto-Response Strategy (Wait for AI)
    Comm-->>Hook: 200 OK
    deactivate Comm
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `WEBHOOK_AUTH_REQ` | Boolean | `true` | Verify signature of inbound webhook payloads. |
| `CONV_MAX_HISTORY` | Integer | `50` | Max messages to retrieve for AI context per thread. |

### 2.3 Conversation Handling (Activity Diagram)
**Overview:** Flow chart describing the triage of incoming user responses and mapping them to their corresponding logical threads.

```mermaid
flowchart TD
    Start((Webhook Received)) --> IdentifyRef[Extract Thread/Sender ID]
    IdentifyRef --> CheckDB{Active Thread Exists?}
    CheckDB -->|Yes| Append[Append to Active Collection]
    CheckDB -->|No| Create[Create New Active Thread]
    Create --> Append
    
    Append --> Rules{Auto Response Rules}
    Rules -->|Ignore| End((End))
    Rules -->|Trigger AI| QueueAI[Queue to AI Engine]
    QueueAI --> WaitResp[Process Request]
    WaitResp --> SendReply[Dispatch Reply & Reset Sliding Timeout]
    SendReply --> DetectEnd{Session Ended / Timeout reached?}
    
    DetectEnd -->|No| End
    DetectEnd -->|Yes| TriggerSummarize[Trigger LLM Summarization]
    
    TriggerSummarize --> CompileThread[Compile Single Conversation File]
    CompileThread --> ExtractFacts[Extract Facts/Numbers]
    ExtractFacts --> MoveToArchive[Move to Archived Collection / Delete Active]
    MoveToArchive --> End
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AUTO_REPLY_ENABLED`| Boolean | `true` | Process conversational AI responses automatically. |
| `IGNORE_SPAM` | Boolean | `true` | Apply rudimentary spam filtering before processing. |

---

## 3. Real-Time Media Pipelines (Voice & Video)

### 3.1 Voice & Video Session Lifecycle (State Diagram)
**Overview:** Manages real-time WebRTC or WebSocket-based voice and video streams. Detailed connection state monitoring is required for handling network fluctuations.

```mermaid
stateDiagram-v2
    [*] --> SESSION_CREATED: Request connection
    SESSION_CREATED --> SESSION_INITIALIZED: ICE negotiated
    SESSION_INITIALIZED --> CONNECTED: Peer connected
    CONNECTED --> IN_PROGRESS: Streaming media
    IN_PROGRESS --> ON_HOLD: Stream muted/paused
    ON_HOLD --> IN_PROGRESS: Stream resumed
    IN_PROGRESS --> ENDED: Call finished normally
    IN_PROGRESS --> FAILED: Connection dropped
    ENDED --> COMPLETED: Media processed
    FAILED --> COMPLETED: Error handled
    COMPLETED --> [*]
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `RTC_ICE_TIMEOUT_MS` | Integer | `10000` | Timeout for ICE candidate gathering negotiation. |
| `RTC_PING_INTERVAL` | Integer | `5000` | Websocket/RTC keep-alive ping interval. |
| `RTC_HOLD_TIMEOUT` | Integer | `300` | Seconds before forcefully ending an ON_HOLD session. |

### 3.2 Streaming Pipeline Lifecycle (State Diagram)
**Overview:** Real-time AI voice interaction requires breaking down continuous audio streams into detectable sentences, processing them through STT/AI/TTS, and streaming back audio.

```mermaid
stateDiagram-v2
    [*] --> STREAM_INITIALIZED: Buffer setup
    STREAM_INITIALIZED --> STREAM_ACTIVE: Ready for chunks
    STREAM_ACTIVE --> SENTENCE_DETECTED: VAD trigger
    SENTENCE_DETECTED --> AI_PROCESSING: STT & LLM
    AI_PROCESSING --> AI_RESPONDING: Output generated
    AI_RESPONDING --> TTS_STREAMING: Streaming audio back
    TTS_STREAMING --> STREAM_ACTIVE: Ready for next sentence
    STREAM_ACTIVE --> STREAM_TERMINATED: End of session
    STREAM_TERMINATED --> [*]
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `VAD_SILENCE_THRES`| Integer | `500` | Milliseconds of silence to trigger sentence bound. |
| `STREAM_CHUNK_SIZE`| Integer | `4096` | Byte size for streaming audio chunk buffering. |
| `AI_RESPONSE_TIMEOUT`|Integer| `3000` | Global timeout for AI stream generation to prevent lag. |

### 3.3 Voice Pipeline Flow (Sequence Diagram)
**Overview:** A low-latency real-time processing pipeline specifically for full-duplex conversational AI voice interactions.

```mermaid
sequenceDiagram
    participant Client
    participant WS as WebSocket API
    participant Audio as VAD & Filter
    participant STT as Speech-to-Text
    participant AI as Core LLM
    participant TTS as Text-to-Speech
    participant DB as MongoDB/MinIO

    Client->>WS: Open Persistent Connection
    Client->>WS: Stream PCM/Opus Audio Chunk
    WS->>Audio: Buffer & Clean Audio
    Audio->>Audio: VAD Detects Speech
    Audio->>STT: Send Speech Segment
    STT-->>WS: Partial Transcript (Streaming)
    STT->>AI: Finalized Text
    AI-->>WS: Stream Response Tokens
    AI->>TTS: Send Text Chunks directly
    TTS-->>Client: Stream Synthesized Audio
    WS->>DB: Append Transcripts to Conversation
    WS->>DB: Upload Raw Audio Recording to MinIO
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `VOICE_CODEC` | String | `opus` | Expected incoming audio codec. |
| `STT_LANGUAGE` | String | `en-US` | Default language for transcription. |
| `TTS_VOICE_ID` | String | `nova` | Voice persona configuration. |

### 3.4 Voice Pipeline Architecture (Block Diagram)
**Overview:** Details the exact buffer chains and processing memory structures used to handle streamed voice packets without disk I/O latency.

```mermaid
flowchart TB
    subgraph Edge
        A[Client Web/App]
    end

    subgraph Streaming Server
        B[WebSocket Gateway]
        C[In-Memory Input Buffer]
        D[DSP / Noise Filter]
        E[VAD Engine]
        F[Sentence Chunk Queue]
    end

    subgraph AI Processing Farm
        G[STT Service]
        H[LLM / Dialogue Engine]
        I[TTS Synthesizer]
    end

    subgraph Storage Tier
        J[(MongoDB)]
        K[(MinIO / S3)]
    end

    A <-->|Audio Stream| B
    B --> C
    C --> D
    D --> E
    E --> F
    F -->|Raw Audio| G
    G -->|Text| H
    H -->|Text| I
    I -->|Synthesized Audio| B
    
    B --> J
    B --> K
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DSP_NOISE_REDUCE`| Boolean | `true` | Enable internal DSP filtering before STT. |
| `BUFFER_POOL_MB` | Integer | `512` | Max RAM allocated for in-memory buffer pool per node. |

### 3.5 Video Pipeline Flow (Sequence Diagram)
**Overview:** A heavy-duty pipeline managing simultaneous audio-video streams, focusing on asynchronous sentiment analysis on video frames alongside standard real-time voice processing.

```mermaid
sequenceDiagram
    participant Client
    participant WS as Media Server
    participant AudioExtract as Multiplexer
    participant Sent as Video Sentiment
    participant AudioPipe as Audio Pipeline
    participant DB as Storage

    Client->>WS: Start WebRTC Video + Audio
    Client->>WS: Stream Tracks
    WS->>AudioExtract: Demux Audio stream
    AudioExtract->>AudioPipe: Forward Audio for STT & AI
    AudioPipe-->>WS: Synthesized Audio Response
    WS->>Sent: Forward Video Frames (1 FPS)
    Sent->>Sent: Analyze Emotions (Happy, Sad, Neutral)
    Sent-->>DB: Async Store Sentiment Meta
    WS->>DB: Save Muxed Output to MinIO
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `VIDEO_FRAMERATE` | Integer | `30` | Inbound expected framerate. |
| `SENTIMENT_FPS` | Integer | `1` | Frames per second extracted for ML emotion tracking. |
| `STORAGE_BUCKET` | String | `qf-media`| MinIO storage bucket name. |

### 3.6 Video Pipeline Architecture (Block Diagram)
**Overview:** Illustrates concurrent processing tracks used when video streaming is active, splitting workloads between media processing, semantic NLP, and ML image inspection.

```mermaid
flowchart LR
    A[Client WebRTC] -->|Stream| B[Media Server]
    
    B -->|Audio Track| C[Audio Pipeline Component]
    C -->|Response| B
    
    B -->|Video Frames| D[Sentiment Engine]
    D -->|Analytics| E[(NoSQL DB)]
    C -->|Transcript| E
    
    B -->|Muxed Recording| F[(Blob Storage)]
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `VIDEO_BITRATE` | Integer | `1500000` | Target transcoding bitrate (bps). |
| `ENABLE_RECORDING`| Boolean | `true` | Save media feeds automatically to blob storage. |

---

## 4. Infrastructure & Buffering

### 4.1 Queue & Buffer Architecture (Block Diagram)
**Overview:** Focuses strictly on the specialized data structures utilized to handle real-time streaming constraints in the voice module, ensuring backpressure limits are respected.

```mermaid
flowchart TB
    A[Incoming Network Stream] --> |Push| B[Input Ring Buffer]
    B --> |Pop| C[Sentence Detection Queue]
    
    C --> D[STT Processing Queue]
    D --> E[AI Generator Queue]
    E --> F[TTS Synthesis Queue]
    
    F --> |Push| G[Output Ring Buffer]
    G --> |Stream| H[Outgoing Network Interface]
    
    %% Scaling indicators
    style B stroke:#333,stroke-dasharray: 5 5
    style G stroke:#333,stroke-dasharray: 5 5
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `RING_BUFFER_SIZE`| Integer | `16384` | Bytes per pre-allocated ring buffer. |
| `QUEUE_TIMEOUT_MS`| Integer | `10000` | How long an item can sit in a queue before dropping. |
| `ENABLE_BACKPRESSURE`| Boolean| `true` | Send rate-limiting events to clients when buffers fill. |

---

## 5. Database Architecture & Schemas

### 5.1 Final Database Architecture (Block Diagram)
**Overview:** This diagram visualizes the high-level storage interaction between the communication orchestrator, the conversational AI layers, and the persistent storage (MongoDB for state/metadata and Vector DB for semantic search and summaries).

```mermaid
flowchart TD
    subgraph Communication Engine
        A[Communication Orchestrator]
        B[AI / LLM Summarizer]
    end

    subgraph MongoDB Storage Cluster
        C[(Communications Log DB)]
        D[(Active Threads DB)]
        E[(Archived Threads DB)]
    end

    subgraph Vector Database Cluster
        F[(Tenant Namespace)]
        G[Agent Sub-namespace]
        H[Conversation Vectors]
    end

    A -->|1. Store generic messages| C
    A <-->|2. Maintain ongoing chat| D
    B -->|3. Compile & Archive thread| E
    B -->|4. Embed & Store Summary| H
    D -.->|Delete upon archival| D
    
    F --> G
    G --> H
```

### 5.2 MongoDB Schema definitions
**Overview:** Details the physical collection structures inside MongoDB. Distinct collections are utilized to isolate short-lived active data from long-term archival and generic communications.

#### Schema: `communications_log` (Collection)
Stores all generic, one-way outbound or inbound communications (e.g. OTPs, marketing blasts) separately from dynamic conversational threads.
```json
{
  "_id": "ObjectId",
  "transaction_id": "String",
  "recipient": "String",
  "status": "String (SENT, DELIVERED, FAILED)",
  "channel_used": "String (SMS, EMAIL, WHATSAPP, PUSH)",
  "provider_id": "String (twilio-01, sendgrid-main)",
  "payload": "Object",
  "created_at": "Timestamp",
  "updated_at": "Timestamp"
}
```

#### Schema: `threads_active` & `threads_archived` (Collections)
Stores bi-directional conversational contexts. Active threads live in `threads_active`; upon closure and summarization, they are migrated to `threads_archived`.
```json
{
  "_id": "ObjectId",
  "tenant_id": "String",
  "agent_id": "String",
  "status": "String (ACTIVE, CLOSED, ARCHIVED)",
  "channel_used": "String (WEB, VOICE, VIDEO, SMS)",
  "provider_id": "String (qf-internal, twilio, zoom)",
  "messages": [
    {
      "sender": "String",
      "content": "String",
      "timestamp": "Timestamp"
    }
  ],
  "analytics": {
    "dominant_sentiment": "String (Positive, Neutral, Negative)",
    "sentiment_percentage": "Float (e.g. 87.5)",
    "extracted_facts": ["String"],
    "extracted_numbers": ["Number"]
  },
  "created_at": "Timestamp",
  "closed_at": "Timestamp"
}
```

### 5.3 Vector DB Hierarchical Structure
**Overview:** Detailed breakdown of the specialized folder/namespace hierarchy utilized to inject semantic, search-ready embeddings of compiled conversation summaries.

**Hierarchy Routing:** `/{Tenant_ID}/{Agent_ID}/{Conversation_ID}`

```json
{
  "namespace": "tenant_a4b9c",
  "metadata": {
    "agent_name": "Sales_Bot_01",
    "agent_id": "agt_99x",
    "conversation_id": "conv_8841abc",
    "channel_used": "VOICE"
  },
  "vectors": [
    {
      "id": "summary_chunk_1",
      "values": [0.12, 0.44, -0.65, ...], 
      "metadata": {
        "text": "User agreed to purchase the enterprise tier after discussing compliance requirements.",
        "fact_type": "decision"
      }
    }
  ]
}
```

**Configuration Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `MONGO_COMM_COLLECTION`| String | `communications_log` | Collection isolated for stateless/generic messages. |
| `VECTOR_DIMENSIONS` | Integer | `1536` | Embedding vector size (e.g. for OpenAI `text-embedding-ada-002`). |
| `DB_SENTIMENT_TRACK` | Boolean | `true` | Aggregate dominant sentiment and percentage upon archiving. |

### 5.4 Database Entity-Relationship (ER) Diagrams

**Overview:** Below are the Entity-Relationship models defining the logical data structures and relationships for PostgreSQL (core relations/configuration), MongoDB (document storage), and Vector DB (hierarchical semantic storage).

#### 1. PostgreSQL ER Diagram (Core Relations)
Manages the structured relational data including tenants, agents, users, and provider configurations.
```mermaid
erDiagram
    TENANT ||--o{ AGENT : owns
    TENANT ||--o{ USER : contains
    TENANT ||--o{ PROVIDER_CONFIG : configures
    AGENT ||--o{ PASSPORT : uses
    
    TENANT {
        uuid id PK
        string name
        timestamp created_at
    }
    AGENT {
        uuid id PK
        uuid tenant_id FK
        string name
        string capabilities
    }
    USER {
        uuid id PK
        uuid tenant_id FK
        string email
        string phone
    }
    PROVIDER_CONFIG {
        uuid id PK
        uuid tenant_id FK
        string provider_type
        json credentials
        int priority
    }
    PASSPORT {
        uuid id PK
        uuid agent_id FK
        string auth_token
        timestamp expires_at
    }
```

#### 2. MongoDB Logical ER Diagram (Document Storage)
Illustrates how the document-based collections (`communications_log`, `threads_active`, `threads_archived`) logically relate to their embedded nested objects (`messages` and `analytics`).
```mermaid
erDiagram
    THREADS_ACTIVE ||--o{ MESSAGE : embeds
    THREADS_ACTIVE ||--o| ANALYTICS : embeds
    THREADS_ARCHIVED ||--o{ MESSAGE : embeds
    THREADS_ARCHIVED ||--o| ANALYTICS : embeds
    
    COMMUNICATIONS_LOG {
        ObjectId _id PK
        string transaction_id
        string recipient
        string status
        string channel_used
        string provider_id
    }
    THREADS_ACTIVE {
        ObjectId _id PK
        string tenant_id
        string agent_id
        string status
        string channel_used
        string provider_id
    }
    THREADS_ARCHIVED {
        ObjectId _id PK
        string tenant_id
        string agent_id
        string status
        string channel_used
        string provider_id
        timestamp closed_at
    }
    MESSAGE {
        string sender
        string content
        timestamp timestamp
    }
    ANALYTICS {
        string dominant_sentiment
        float sentiment_percentage
        string[] extracted_facts
        float[] extracted_numbers
    }
```

#### 3. Vector DB Logical ER Diagram (Hierarchical Namespaces)
Details the namespace routing logic leading down to individual vector chunks containing spatial float arrays and semantic metadata.
```mermaid
erDiagram
    TENANT_NAMESPACE ||--|{ AGENT_NAMESPACE : contains
    AGENT_NAMESPACE ||--|{ CONVERSATION_VECTORS : groups
    CONVERSATION_VECTORS ||--|{ VECTOR_CHUNK : stores
    
    TENANT_NAMESPACE {
        string namespace_id PK "e.g. tenant_a4b9c"
    }
    AGENT_NAMESPACE {
        string metadata_agent_id PK "e.g. agt_99x"
    }
    CONVERSATION_VECTORS {
        string metadata_conversation_id PK "e.g. conv_8841abc"
        string metadata_channel_used
        string metadata_agent_name
    }
    VECTOR_CHUNK {
        string id PK "chunk ID"
        float[] values "1536d float array"
        string metadata_text "summary segment"
        string metadata_fact_type "e.g. decision"
    }
```

---

## 6. Project Folder Structure & Dependencies

### 6.1 Communication Module Folder Structure
To maintain Clean Architecture consistency with the `Passport` and `Infrastructure` modules, the `Communication` module will implement the following `.csproj` boundaries inside the `src/Communication` directory:

```text
src/
└── Communication/
    ├── QuestFlag.Communication.ApiCore     (API Controllers, HTTP Routing logic)
    ├── QuestFlag.Communication.Application (Use Cases, CQRS Handlers, Validation)
    ├── QuestFlag.Communication.Client      (HTTP/gRPC SDK for other microservices)
    ├── QuestFlag.Communication.Core        (Data Access, EF Core, Mongo/Vector implementations)
    ├── QuestFlag.Communication.Domain      (Entities, Interfaces, Enums, Exceptions)
    ├── QuestFlag.Communication.Services    (Executable API/Worker Host, Dependency Injection)
    └── QuestFlag.Communication.WebApp      (Optional: UI for agent dashboard)
```

### 6.2 Cross-Project Updates Required

#### Updates to `Passport` Project
The Authentication module will occasionally need to trigger transactional messages (e.g., OTPs, Welcome Emails, Password Resets).
1. Add a direct project reference to `QuestFlag.Communication.Client` within `QuestFlag.Passport.Core` or `QuestFlag.Passport.Services`.
2. Ensure `Passport` emits `UserCreated` or `OtpRequested` events onto the Message Broker (Kafka/RabbitMQ) so that the `Communication` workers can trigger these emails asynchronously without tightly coupling HTTP requests.

#### Updates to `Infrastructure` Project
The Core Infrastructure serves shared connections and utilities.
1. Ensure the shared Message Broker configurations in `QuestFlag.Infrastructure.Core` are updated to support the new `qf-communication-tasks` and `qf-dlq` topics.
2. Ensure standard provider SDKs (Twilio, SendGrid) configurations or HttpClient wrappers can be injected easily via the `QuestFlag.Infrastructure.Services` DI container, or strictly scoped to the `Communication` module if preferred.

### 6.3 Internal Dependencies & External Packages Breakdown

Below details exactly which sub-projects reference which dependencies:

#### 1. `QuestFlag.Communication.Domain`
* **Internal Refs**: None. This is the innermost ring.
* **External Packages**: None (Pure C# models).

#### 2. `QuestFlag.Communication.Application`
* **Internal Refs**: `QuestFlag.Communication.Domain`
* **External Packages**: 
  - `MediatR` (CQRS pattern)
  - `FluentValidation.DependencyInjectionExtensions`

#### 3. `QuestFlag.Communication.Core`
* **Internal Refs**: `QuestFlag.Communication.Domain`, `QuestFlag.Communication.Application`
* **External Packages**: 
  - `Microsoft.EntityFrameworkCore` & `Npgsql.EntityFrameworkCore.PostgreSQL` (Relational data)
  - `MongoDB.Driver` (Document storage for logs and threads)
  - Vector DB SDK (e.g. `Pinecone.NET` or `Qdrant.Client`)
  - AI SDKs (e.g. `Azure.AI.OpenAI` or similar for Summarizations / Embeddings)

#### 4. `QuestFlag.Communication.ApiCore`
* **Internal Refs**: `QuestFlag.Communication.Domain`, `QuestFlag.Communication.Application`
* **External Packages**: 
  - `Microsoft.AspNetCore.Mvc.Core`

#### 5. `QuestFlag.Communication.Client`
* **Internal Refs**: `QuestFlag.Communication.Domain` (For sharing DTOs/Contracts)
* **External Packages**: 
  - `System.Net.Http.Json`

#### 6. `QuestFlag.Communication.Services` (The Host application)
* **Internal Refs**: `QuestFlag.Communication.ApiCore`, `QuestFlag.Communication.Application`, `QuestFlag.Communication.Core`, `QuestFlag.Infrastructure.Client`
* **External Packages**: 
  - `Microsoft.AspNetCore.Authentication.JwtBearer`
  - `Swashbuckle.AspNetCore`
  - `Microsoft.EntityFrameworkCore.Tools` / `Design`
