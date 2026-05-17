# Tópicos Kafka

## `user-activity-events`

Eventos de navegação e comportamento do usuário.

| EventType | Descrição | Key sugerida |
|-----------|-----------|--------------|
| `ProductViewed` | Visualização de produto | `EventId` |
| `ProductAddedToCart` | Item adicionado ao carrinho | `EventId` |
| `CheckoutStarted` | Início do checkout | `EventId` |
| `UserLoggedIn` | Login do usuário | `EventId` |

- **Partições**: 3 (permite paralelismo no consumer group)
- **Replicação**: 1 (ambiente local/Docker)
- **Serialização**: JSON (camelCase)
- **Headers**: `event-type`, `correlation-id`

## `order-events`

Eventos do domínio de pedidos.

| EventType | Descrição |
|-----------|-----------|
| `OrderCompleted` | Pedido finalizado com sucesso |

- **Partições**: 3
- Separação semântica: pedidos podem ter SLAs e consumidores diferentes no futuro.

## `analytics-dead-letter`

Mensagens que falharam após retries no consumer.

Payload inclui: tópico original, tipo do evento, payload bruto, erro e timestamp.

- **Partições**: 1 (inspeção manual / ferramentas de replay)
- Não faz parte do fluxo feliz; usado para observabilidade e correção.

## Mapeamento evento → tópico

| Evento | Tópico |
|--------|--------|
| ProductViewed | user-activity-events |
| ProductAddedToCart | user-activity-events |
| CheckoutStarted | user-activity-events |
| UserLoggedIn | user-activity-events |
| OrderCompleted | order-events |

## Consumer group

- **Group ID**: `analytics-consumer-group`
- **Instâncias**: escalar `analytics-consumer` no Compose aumenta consumidores no mesmo group (partições limitam paralelismo máximo).
