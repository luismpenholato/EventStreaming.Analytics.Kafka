# ADR 0003: Separate ingestion from Query API

## Status

Accepted

## Context

Combining event ingestion and metrics querying in the same API couples latencies and makes it harder to scale reads and writes independently.

## Decision

- `Tracking.Api` — write-facing HTTP ingestion only (Outbox persistence).
- `Analytics.Consumer` — stream processing and metric persistence.
- `Analytics.QueryApi` — read-only access to PostgreSQL.

## Consequences

- Eventual consistency between POST and subsequent GET of metrics.
- Independent deployment and scaling paths.
- Clear pedagogical separation (light CQRS).
