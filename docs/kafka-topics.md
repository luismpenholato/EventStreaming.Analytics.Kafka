# Kafka topics

## `user-activity-events`

User navigation and behavior events.

| EventType | Description | Suggested key |
|-----------|-------------|---------------|
| `ProductViewed` | Product page view | `EventId` |
| `ProductAddedToCart` | Item added to cart | `EventId` |
| `CheckoutStarted` | Checkout started | `EventId` |
| `UserLoggedIn` | User login | `EventId` |

- **Partitions**: 3 (enables consumer group parallelism)
- **Replication**: 1 (local / Docker demo)
- **Serialization**: JSON (camelCase)
- **Headers**: `event-type`, `correlation-id`

## `order-events`

Order domain events.

| EventType | Description |
|-----------|-------------|
| `OrderCompleted` | Order successfully completed |

- **Partitions**: 3
- Semantic separation: orders may have different SLAs and consumers in the future.

## `analytics-dead-letter`

Messages that failed after consumer retries.

Payload includes: original topic, event type, raw payload, error, and timestamp.

- **Partitions**: 1 (manual inspection / replay tooling)
- Not part of the happy path; used for observability and remediation.

## Event → topic mapping

| Event | Topic |
|-------|-------|
| ProductViewed | user-activity-events |
| ProductAddedToCart | user-activity-events |
| CheckoutStarted | user-activity-events |
| UserLoggedIn | user-activity-events |
| OrderCompleted | order-events |

Topic names are centralized in `BuildingBlocks.Contracts.Topics.KafkaTopics`.

## Consumer group

- **Group ID**: `analytics-consumer-group`
- **Scaling**: increasing `analytics-consumer` replicas in Compose adds consumers to the same group (partition count caps max parallelism).
