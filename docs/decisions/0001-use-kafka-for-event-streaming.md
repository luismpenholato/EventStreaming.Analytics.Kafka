# ADR 0001: Use Kafka for event streaming

## Status

Accepted

## Context

We need to demonstrate high-volume architecture with asynchronous processing, multiple consumers, and replay capability for analytics.

## Decision

Adopt Apache Kafka via `Confluent.Kafka` as the event backbone.

## Consequences

- Immutable logs enable metric reprocessing.
- Consumer groups scale processing horizontally.
- Local operation via Docker Compose with Zookeeper + Kafka.
- Steeper learning curve than a simple AMQP queue.
