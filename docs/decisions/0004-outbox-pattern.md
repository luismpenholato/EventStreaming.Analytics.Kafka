# ADR 0004: Outbox Pattern in Tracking API

## Status

Accepted

## Problem addressed

Publishing directly to Kafka inside the HTTP request creates consistency risks:

- The API may return success and the broker publish may fail afterward.
- Without a local transaction, there is no atomic guarantee between persistence and Kafka send.
- Transient network failures to the broker can lose events without a recoverable trace on the API side.

For a production-like portfolio, we need **durable ingestion** decoupled from broker latency.

## Alternatives considered

| Alternative | Why not chosen |
|-------------|----------------|
| Direct Kafka publish on POST | No guarantee between HTTP 202 and message on topic |
| Transactional Outbox + Debezium CDC | Excellent at scale, but adds operational complexity (Connect, internal topics) for this demo |
| MassTransit / bus abstraction | Project goal: demonstrate native Kafka and `Confluent.Kafka` |
| Saga / process manager | Overengineering for fire-and-forget ingestion |

## Decision

1. `POST /api/events/*` validates the DTO and persists to `outbox_messages` via `IOutboxWriter`.
2. The API returns **202 Accepted** with `eventId` and `correlationId` after PostgreSQL commit.
3. `OutboxPublisherWorker` delegates to `IOutboxPublisher`, which publishes pending batches to Kafka.
4. Control fields: `created_at`, `processed_at`, `retry_count`, `last_error`.
5. Unique index on `event_id` prevents duplicate outbox rows for the same event.

## Positive consequences

- **Durable** ingestion even when Kafka is temporarily unavailable.
- Reprocessing and observability via the outbox table.
- HTTP layer does not depend on Kafka producer details.
- Aligns with patterns used in production event-driven systems.

## Trade-offs

| Trade-off | Impact |
|-----------|--------|
| Eventual latency to Kafka | Query API metrics do not reflect POST immediately |
| 202 ≠ message on topic | API consumers must understand Outbox semantics |
| Worker polling (~2s) | Not real-time; acceptable for analytics |
| Shared PostgreSQL with analytics | Operational simplicity; not full bounded-context isolation |
| Worker hosted in Tracking API process | In larger production systems, relay could be a dedicated service |

## Code references

- `Tracking.Application` → `IOutboxWriter`
- `Tracking.Infrastructure` → `OutboxWriter`, `OutboxPublisher`, `OutboxPublisherWorker`
- Migration `AddOutbox` → `outbox_messages` table
