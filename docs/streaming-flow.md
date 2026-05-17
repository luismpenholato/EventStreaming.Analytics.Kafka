# Fluxo de streaming

## 1. Ingestão (Tracking API + Outbox)

1. Cliente envia `POST /api/events/{tipo}` com DTO validado (FluentValidation).
2. Application mapeia DTO → `IntegrationEvent` (contrato compartilhado).
3. `IOutboxWriter` persiste em `outbox_messages` (PostgreSQL).
4. API responde **202 Accepted** com `eventId` e `correlationId`.

> **Importante:** 202 significa evento **persistido no Outbox**, não publicado no Kafka ainda.

5. `OutboxPublisherWorker` chama `IOutboxPublisher` em background.
6. Mensagens publicadas no tópico correto via `IIntegrationEventPublisher`.

A API **não** grava métricas — apenas enfileira para o stream.

## 2. Stream (Kafka)

- Eventos ficam no log até expirarem (retenção configurável no broker).
- Offsets gerenciados por partition + consumer group.
- Commit manual após processamento bem-sucedido.

## 3. Processamento (Analytics Consumer)

1. `AnalyticsKafkaConsumer` faz poll em `user-activity-events` e `order-events`.
2. `IntegrationEventRegistry` deserializa JSON → `IntegrationEvent`.
3. `EventDispatcher` + `EventHandlerInvoker` roteiam para `IEventHandler<T>`.
4. `ProcessIdempotentlyAsync`: transação com insert em `processed_events` + métricas.
5. Retry com backoff; falha definitiva → `analytics-dead-letter`.

## 4. Consulta (Query API)

1. Cliente chama `GET /api/analytics/*`.
2. `DapperAnalyticsQueries` lê tabelas materializadas (somente leitura).
3. Sem acesso ao Kafka — latência previsível.

## Ambiente Docker

```bash
docker compose up --build
```

| Serviço | Porta |
|---------|-------|
| Tracking API | 5001 |
| Query API | 5002 |

Exemplos HTTP: [requests/event-streaming.http](requests/event-streaming.http)

## Métricas materializadas

| Métrica | Origem dos eventos |
|---------|-------------------|
| Visualizações por produto | ProductViewed |
| Adições ao carrinho | ProductAddedToCart |
| Checkouts iniciados | CheckoutStarted |
| Pedidos concluídos | OrderCompleted |
| Logins | UserLoggedIn |
| Taxa de conversão | views → orders (simplificada) |
| Eventos por tipo | contador global por EventType |
| Eventos por período | bucket horário (OccurredAt) |
