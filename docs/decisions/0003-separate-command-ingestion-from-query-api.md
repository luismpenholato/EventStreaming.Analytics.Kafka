# ADR 0003: Separar ingestão (command) da Query API

## Status

Aceito

## Contexto

Misturar ingestão de eventos e consulta de métricas na mesma API acopla latências e dificulta escalar leitura e escrita independentemente.

## Decisão

- `Tracking.Api` — apenas publicação no Kafka.
- `Analytics.Consumer` — processamento e persistência.
- `Analytics.QueryApi` — apenas leitura do PostgreSQL.

## Consequências

- Modelo eventual consistency entre POST do evento e GET da métrica.
- Deploy e scaling independentes.
- Clareza pedagógica para portfólio (CQRS light).
