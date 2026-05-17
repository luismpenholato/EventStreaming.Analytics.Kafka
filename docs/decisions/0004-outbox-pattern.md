# ADR 0004: Outbox Pattern na Tracking API

## Status

Aceito

## Problema resolvido

Publicar diretamente no Kafka dentro do request HTTP cria risco de inconsistência:

- A API pode retornar sucesso e a publicação no broker falhar em seguida.
- Em cenários com transações de banco, não há garantia atômica entre persistência local e envio ao Kafka.
- Falhas transitórias de rede para o broker perdem o evento sem rastro recuperável no lado da API.

Para um portfólio que simula produção, precisamos de **durabilidade na ingestão** desacoplada da latência do broker.

## Alternativas consideradas

| Alternativa | Por que não foi escolhida |
|-------------|---------------------------|
| Publish direto no Kafka no POST | Sem garantia entre HTTP 202 e mensagem no tópico |
| Transactional Outbox + Debezium CDC | Excelente em escala, mas aumenta operação (connect, tópicos internos) para o escopo do portfólio |
| MassTransit / abstração de bus | Restrição do projeto: demonstrar Kafka nativo e Confluent.Kafka |
| Saga / process manager | Overengineering para ingestão fire-and-forget |

## Decisão tomada

1. `POST /api/events/*` valida o DTO e persiste em `outbox_messages` via `IOutboxWriter`.
2. A API responde **202 Accepted** com `eventId` e `correlationId` após commit no PostgreSQL.
3. `OutboxPublisherWorker` delega para `IOutboxPublisher`, que publica lotes pendentes no Kafka.
4. Campos de controle: `created_at`, `processed_at`, `retry_count`, `last_error`.
5. Índice único em `event_id` evita duplicar a mesma mensagem de outbox.

## Consequências positivas

- Ingestão **durável** mesmo se Kafka estiver indisponível temporariamente.
- Reprocessamento e observabilidade via tabela de outbox.
- Separação clara: HTTP não conhece detalhes do producer Kafka.
- Alinhado a padrões vistos em sistemas event-driven em produção.

## Trade-offs

| Trade-off | Impacto |
|-----------|---------|
| Latência eventual até o Kafka | Métricas na Query API não refletem o POST imediatamente |
| 202 ≠ mensagem no tópico | Quem consome a API precisa entender o Outbox |
| Polling do worker (2s) | Não é real-time; aceitável para analytics |
| Mesmo PostgreSQL do analytics | Simplicidade operacional; não é isolamento total de bounded contexts |
| Worker + publisher no mesmo processo da API | Em produção grande, o relay poderia ser serviço dedicado |

## Referências no código

- `Tracking.Application` → `IOutboxWriter`
- `Tracking.Infrastructure` → `OutboxWriter`, `OutboxPublisher`, `OutboxPublisherWorker`
- Migration `AddOutbox` → tabela `outbox_messages`
