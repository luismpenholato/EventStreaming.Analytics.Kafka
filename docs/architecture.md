# Architecture — EventStreaming.Analytics.Kafka

## Overview

Event streaming analytics platform that simulates user-behavior tracking in an e-commerce scenario. The system separates ingestion, relay, processing, and querying with production-like patterns:

| Layer | Service | Responsibility |
|-------|---------|----------------|
| Ingestion | `Tracking.Api` | Receive HTTP events, validate, persist to **Outbox** |
| Relay | `OutboxPublisherWorker` | Publish pending outbox rows to Kafka via `IOutboxPublisher` |
| Processing | `Analytics.Consumer` | Consume stream, aggregate metrics, transactional idempotency |
| Query | `Analytics.QueryApi` | Expose metrics via **Dapper** (read-only) |

## Outbox flow

```mermaid
sequenceDiagram
    participant Client
    participant TrackingAPI as Tracking API
    participant Outbox as outbox_messages
    participant Worker as OutboxPublisherWorker
    participant Kafka
    participant Consumer as Analytics Consumer
    participant DB as PostgreSQL
    participant QueryAPI as Query API

    Client->>TrackingAPI: POST /api/events/*
    TrackingAPI->>Outbox: INSERT (transaction)
    TrackingAPI-->>Client: 202 Accepted (eventId, correlationId)

    loop every ~2s
        Worker->>Outbox: SELECT pending
        Worker->>Kafka: publish
        Worker->>Outbox: UPDATE processed_at
    end

    Kafka->>Consumer: poll
    Consumer->>DB: transaction (processed_events + metrics)
    Consumer->>Kafka: commit offset

    Client->>QueryAPI: GET /api/analytics/*
    QueryAPI->>DB: SELECT (Dapper)
    QueryAPI-->>Client: 200 JSON
```

## Consumer idempotency

```mermaid
flowchart TD
    A[Kafka message] --> B[EventDispatcher]
    B --> C[ProcessIdempotentlyAsync]
    C --> D{INSERT processed_events}
    D -->|unique violation| E[Skip duplicate]
    D -->|success| F[Update metrics]
    F --> G[COMMIT transaction]
    G --> H[Commit Kafka offset]
    C -->|error after retries| I[analytics-dead-letter]
```

## Bounded contexts

- **Tracking**: validation, outbox, async publication.
- **Analytics**: stream processing, aggregation, `processed_events` ledger.
- **Query**: SQL reads via Dapper, no dependency on the write stack.

## Building blocks

| Project | Contents |
|---------|----------|
| `BuildingBlocks.Contracts` | Events, `IIntegrationEventPublisher`, `IntegrationEventRegistry` |
| `BuildingBlocks.Kafka` | Producer, consumer base, retry, DLQ |
| `BuildingBlocks.Domain` | `Result<T>` |
| `BuildingBlocks.Observability` | Serilog, `CorrelationIdMiddleware` |

## Local execution

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| Tracking API | http://localhost:5001 |
| Query API | http://localhost:5002 |

Sample requests: [requests/event-streaming.http](requests/event-streaming.http)

## Decisions

See [decisions/](decisions/) for numbered ADRs.
