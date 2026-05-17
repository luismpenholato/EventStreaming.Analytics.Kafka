# ADR 0001: Usar Kafka para event streaming

## Status

Aceito

## Contexto

Precisamos demonstrar arquitetura de alto volume com processamento assíncrono, múltiplos consumidores e possibilidade de replay para analytics.

## Decisão

Adotar Apache Kafka via `Confluent.Kafka` como backbone de eventos.

## Consequências

- Logs imutáveis permitem reprocessamento de métricas.
- Consumer groups escalam processamento horizontalmente.
- Operação local via Docker Compose com Zookeeper + Kafka.
- Curva de aprendizado maior que uma fila AMQP simples.
