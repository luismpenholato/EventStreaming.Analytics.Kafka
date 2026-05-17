# Arquitetura — EventStreaming.Analytics.Kafka

## Visão geral

Plataforma de analytics baseada em **event streaming** que simula rastreamento de comportamento do usuário em um e-commerce. O sistema separa três responsabilidades com decisões próximas de produção:

| Camada | Serviço | Responsabilidade |
|--------|---------|------------------|
| Ingestão | `Tracking.Api` | Receber eventos HTTP, validar, persistir no **Outbox** |
| Relay | `OutboxPublisherWorker` | Publicar outbox pendente no Kafka (`IOutboxPublisher`) |
| Processamento | `Analytics.Consumer` | Consumir stream, agregar métricas, idempotência |
| Consulta | `Analytics.QueryApi` | Expor métricas via **Dapper** (somente leitura) |

## Fluxo com Outbox

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
    TrackingAPI->>Outbox: INSERT (transação)
    TrackingAPI-->>Client: 202 Accepted (eventId, correlationId)

    loop a cada ~2s
        Worker->>Outbox: SELECT pending
        Worker->>Kafka: publish
        Worker->>Outbox: UPDATE processed_at
    end

    Kafka->>Consumer: poll
    Consumer->>DB: transação (processed_events + métricas)
    Consumer->>Kafka: commit offset

    Client->>QueryAPI: GET /api/analytics/*
    QueryAPI->>DB: SELECT (Dapper)
    QueryAPI-->>Client: 200 JSON
```

## Idempotência do consumer

```mermaid
flowchart TD
    A[Mensagem Kafka] --> B[EventDispatcher]
    B --> C[ProcessIdempotentlyAsync]
    C --> D{INSERT processed_events}
    D -->|unique violation| E[Skip - duplicata]
    D -->|sucesso| F[Atualizar métricas]
    F --> G[COMMIT transação]
    G --> H[Commit offset Kafka]
    C -->|erro após retries| I[analytics-dead-letter]
```

## Bounded contexts

- **Tracking**: validação, outbox, publicação assíncrona.
- **Analytics**: processamento, agregação, `processed_events`.
- **Query**: leitura SQL via Dapper, sem dependência de write stack.

## Building blocks

| Projeto | Conteúdo |
|---------|----------|
| `BuildingBlocks.Contracts` | Eventos, `IIntegrationEventPublisher`, `IntegrationEventRegistry` |
| `BuildingBlocks.Kafka` | Producer, consumer base, retry, DLQ |
| `BuildingBlocks.Domain` | `Result<T>` |
| `BuildingBlocks.Observability` | Serilog, `CorrelationIdMiddleware` |

## Execução local

```bash
docker compose up --build
```

| Serviço | URL |
|---------|-----|
| Tracking API | http://localhost:5001 |
| Query API | http://localhost:5002 |

Requisições de exemplo: [requests/event-streaming.http](requests/event-streaming.http)

## Decisões

Ver [decisions/](decisions/) para ADRs numerados.
