# ADR 0002: Use consumer groups

## Status

Accepted

## Context

Multiple `Analytics.Consumer` instances must share partitions without processing the same event twice within the same group.

## Decision

Use `analytics-consumer-group` with manual offset commit after processing and application-level idempotency by `EventId`.

## Consequences

- Parallelism is limited by partition count (3 per topic in the demo).
- Rebalancing occurs when instances join or leave the group.
- Application idempotency remains required (at-least-once delivery).
