# Contributing

Thanks for your interest in **EventStreaming.Analytics.Kafka**.

This is a **portfolio / demo project**. Contributions are welcome, but scope is intentionally focused on event streaming patterns with .NET, Kafka, and PostgreSQL.

## Before you start

1. Open an issue to describe the change (bug, doc fix, small improvement).
2. Keep pull requests **small and focused**.
3. Do not introduce new frameworks or infrastructure unless discussed first.
4. Preserve existing architectural boundaries (Tracking / Analytics / Query / BuildingBlocks).

## Development setup

Requires .NET 10 SDK and Docker (for integration tests and Compose).

```bash
dotnet restore
dotnet build
dotnet test
```

Integration tests use **Testcontainers** and require Docker to be running.

Local stack:

```bash
docker compose up --build
```

See [README.md](README.md) for endpoints and validation steps.

## Code guidelines

- Match existing naming, folder structure, and patterns.
- Keep business rules unchanged unless the issue explicitly requires it.
- Prefer explicit handlers and simple DI over new abstractions.
- Add or update tests when behavior changes.
- Update documentation when public behavior or setup changes.

## Pull request checklist

- [ ] `dotnet build` passes
- [ ] Relevant tests pass (`dotnet test`)
- [ ] Documentation updated if needed
- [ ] No secrets or local config files committed (`.env`, credentials)

## Questions

Use GitHub Issues for bugs, questions, and improvement ideas.
