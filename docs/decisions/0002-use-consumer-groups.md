# ADR 0002: Usar consumer groups

## Status

Aceito

## Contexto

Múltiplas instâncias do `Analytics.Consumer` devem dividir partições sem processar o mesmo evento duas vezes no mesmo group.

## Decisão

Usar `analytics-consumer-group` com commit manual após processamento e idempotência por `EventId`.

## Consequências

- Paralelismo limitado ao número de partições (3 por tópico).
- Rebalanceamento ao subir/descer instâncias.
- Idempotência na aplicação continua necessária (at-least-once delivery).
