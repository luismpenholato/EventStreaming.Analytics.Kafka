# Streaming flow

## 1. Ingestion (Tracking API + Outbox)

1. Client sends `POST /api/events/{type}` with a validated DTO (FluentValidation).
2. Application maps DTO → `IntegrationEvent` (shared contract).
3. `IOutboxWriter` persists to `outbox_messages` (PostgreSQL).
4. API returns **202 Accepted** with `eventId` and `correlationId`.

> **Important:** `202` means the event is **stored in the Outbox**, not yet published to Kafka.

5. `OutboxPublisherWorker` invokes `IOutboxPublisher` in the background.
6. Messages are published to the correct topic via `IIntegrationEventPublisher`.

The API does **not** write metrics — it only enqueues for the stream.

## 2. Stream (Kafka)

- Events remain in the log until retention expires (broker-configurable).
- Offsets are managed per partition and consumer group.
- Manual commit after successful processing.

## 3. Processing (Analytics Consumer)

1. `AnalyticsKafkaConsumer` polls `user-activity-events` and `order-events`.
2. `IntegrationEventRegistry` deserializes JSON → `IntegrationEvent`.
3. `EventDispatcher` + `EventHandlerInvoker` route to `IEventHandler<T>`.
4. `ProcessIdempotentlyAsync`: single transaction with insert into `processed_events` + metric updates.
5. Retry with backoff; final failure → `analytics-dead-letter`.

## 4. Query (Query API)

1. Client calls `GET /api/analytics/*`.
2. `DapperAnalyticsQueries` reads materialized tables (read-only).
3. No Kafka access — predictable read latency.

## Docker environment

```bash
docker compose up --build
```

| Service | Port |
|---------|------|
| Tracking API | 5001 |
| Query API | 5002 |

HTTP samples: [requests/event-streaming.http](requests/event-streaming.http)

## Materialized metrics

| Metric | Source events |
|--------|---------------|
| Product views | `ProductViewed` |
| Cart additions | `ProductAddedToCart` |
| Checkouts started | `CheckoutStarted` |
| Orders completed | `OrderCompleted` |
| Logins | `UserLoggedIn` |
| Conversion rate | views → orders (simplified) |
| Events by type | global counter per `EventType` |
| Events by period | hourly bucket (`OccurredAt`) |
