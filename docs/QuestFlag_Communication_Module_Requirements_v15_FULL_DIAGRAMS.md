# QuestFlag Communication Platform -- FULL Architecture & Requirements (v15)

Version: 15.0 Date: 2026-03-01

====================================================================
SECTION 1 -- GENERIC COMMUNICATION LIFECYCLE (STATE DIAGRAM)
====================================================================

``` mermaid
stateDiagram-v2
[*] --> CREATED
CREATED --> VALIDATED
VALIDATED --> QUEUED
QUEUED --> SENDING
SENDING --> SENT
SENT --> DELIVERED
DELIVERED --> READ
SENDING --> RETRYING
RETRYING --> SENDING
RETRYING --> FAILED
FAILED --> DEAD_LETTERED
CREATED --> CANCELLED
VALIDATED --> CANCELLED
```

====================================================================
SECTION 2 -- CONVERSATION LIFECYCLE (STATE DIAGRAM)
====================================================================

``` mermaid
stateDiagram-v2
[*] --> CONVERSATION_CREATED
CONVERSATION_CREATED --> ACTIVE
ACTIVE --> WAITING_FOR_REPLY
WAITING_FOR_REPLY --> ACTIVE
ACTIVE --> CLOSED
CLOSED --> ARCHIVED
```

====================================================================
SECTION 3 -- VOICE / VIDEO SESSION LIFECYCLE (STATE DIAGRAM)
====================================================================

``` mermaid
stateDiagram-v2
[*] --> SESSION_CREATED
SESSION_CREATED --> SESSION_INITIALIZED
SESSION_INITIALIZED --> CONNECTED
CONNECTED --> IN_PROGRESS
IN_PROGRESS --> ON_HOLD
ON_HOLD --> IN_PROGRESS
IN_PROGRESS --> ENDED
ENDED --> COMPLETED
IN_PROGRESS --> FAILED
```

====================================================================
SECTION 4 -- STREAMING PIPELINE LIFECYCLE (STATE DIAGRAM)
====================================================================

``` mermaid
stateDiagram-v2
[*] --> STREAM_INITIALIZED
STREAM_INITIALIZED --> STREAM_ACTIVE
STREAM_ACTIVE --> SENTENCE_DETECTED
SENTENCE_DETECTED --> AI_PROCESSING
AI_PROCESSING --> AI_RESPONDING
AI_RESPONDING --> TTS_STREAMING
TTS_STREAMING --> STREAM_ACTIVE
STREAM_ACTIVE --> STREAM_TERMINATED
```

====================================================================
SECTION 5 -- GENERIC COMMUNICATION FLOW (SEQUENCE DIAGRAM)
====================================================================

``` mermaid
sequenceDiagram
Agent->>Communication: Send Message
Communication->>Passport: Validate
Communication->>ProviderResolver: Resolve Provider (Priority)
Communication->>Queue: Enqueue
Worker->>Infra: Call Provider
Infra-->>Worker: Normalized Response
Worker->>MongoDB: Update State
```

====================================================================
SECTION 6 -- GENERIC CONVERSATION FLOW (SEQUENCE DIAGRAM)
====================================================================

``` mermaid
sequenceDiagram
User->>ProviderWebhook: Send Reply
ProviderWebhook->>Communication: Process Inbound
Communication->>MongoDB: Append Conversation
Communication->>Queue: Trigger Auto-Response (Optional)
```

====================================================================
SECTION 7 -- VOICE PIPELINE (SEQUENCE DIAGRAM)
====================================================================

``` mermaid
sequenceDiagram
Client->>WS: Open Persistent Connection
Client->>WS: Stream Audio
WS->>Filter: Clean Audio
Filter->>VAD: Detect Speech
VAD->>STT: Convert to Text
STT-->>WS: Partial Transcript
WS->>AI: Process Text
AI-->>WS: Response Text
WS->>TTS: Convert to Audio
TTS-->>Client: Stream Audio Back
WS->>MongoDB: Append Transcript
WS->>MinIO: Store Recording
```

====================================================================
SECTION 8 -- VIDEO PIPELINE (SEQUENCE DIAGRAM)
====================================================================

``` mermaid
sequenceDiagram
Client->>WS: Open Persistent Video Session
Client->>WS: Stream Video + Audio
WS->>AudioExtractor: Extract Audio
AudioExtractor->>STT: Convert to Text
STT->>AI: Process
AI-->>WS: Response
WS->>VideoSentiment: Analyze Frames
VideoSentiment-->>MongoDB: Store Emotion
WS->>MinIO: Store Recording
```

====================================================================
SECTION 9 -- GENERIC COMMUNICATION (ACTIVITY DIAGRAM)
====================================================================

``` mermaid
flowchart TD
Start --> Validate
Validate --> ResolveProvider
ResolveProvider --> CreateTransaction
CreateTransaction --> QueueMessage
QueueMessage --> WorkerProcessing
WorkerProcessing --> Success
WorkerProcessing --> Failure
Failure --> RetryOrFailover
RetryOrFailover --> Success
Success --> PersistState
PersistState --> End
```

====================================================================
SECTION 10 -- CONVERSATION HANDLING (ACTIVITY DIAGRAM)
====================================================================

``` mermaid
flowchart TD
Start --> ReceiveInbound
ReceiveInbound --> IdentifyConversation
IdentifyConversation --> AppendMessage
AppendMessage --> CheckAutoResponse
CheckAutoResponse --> SendReply
SendReply --> Persist
Persist --> End
```

====================================================================
SECTION 11 -- GENERIC COMMUNICATION PIPELINE (BLOCK DIAGRAM)
====================================================================

``` mermaid
flowchart LR
A[Agent] --> B[Communication Orchestrator]
B --> C[Passport]
C --> D[Provider Resolver]
D --> E[Transaction Manager]
E --> F[Outbound Queue]
F --> G[Worker]
G --> H[Infra Wrapper]
H --> I[External Provider]
G --> J[MongoDB]
```

====================================================================
SECTION 12 -- VOICE PIPELINE (BLOCK DIAGRAM)
====================================================================

``` mermaid
flowchart LR
A[Client Audio] --> B[Persistent WS]
B --> C[Input Buffer]
C --> D[Audio Filter]
D --> E[VAD]
E --> F[Sentence Queue]
F --> G[STT]
G --> H[AI Queue]
H --> I[AI Engine]
I --> J[TTS Queue]
J --> K[TTS Engine]
K --> B
B --> L[MongoDB Transcript]
B --> M[MinIO Recording]
```

====================================================================
SECTION 13 -- VIDEO PIPELINE (BLOCK DIAGRAM)
====================================================================

``` mermaid
flowchart LR
A[Client Video] --> B[Persistent WS]
B --> C[Audio Extractor]
C --> D[Audio Filter]
D --> E[VAD]
E --> F[STT]
F --> G[AI Engine]
G --> H[TTS]
B --> I[Video Sentiment Engine]
I --> J[MongoDB Sentiment]
B --> K[MinIO Recording]
```

====================================================================
SECTION 14 -- QUEUE & BUFFER ARCHITECTURE (BLOCK)
====================================================================

``` mermaid
flowchart TB
A[Input Stream] --> B[Input Buffer Queue]
B --> C[Sentence Queue]
C --> D[AI Processing Queue]
D --> E[TTS Queue]
E --> F[Output Buffer]
```

====================================================================

End of FULL Diagram Restoration Document (v15)
