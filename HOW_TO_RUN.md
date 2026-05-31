# How to Run

Two ways to get the API up: **all-in-Docker** (zero local deps beyond Docker) or **hybrid** (run only the infra in Docker, run the API from your IDE / `dotnet run` for debugging).

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose v2)
- For the hybrid / IDE workflow: [.NET 10 SDK](https://dotnet.microsoft.com/download)

All `docker compose` commands below assume you run them from the **solution root** (`NlayeredArchitectureStarter/`).

---

## Option A — Everything in Docker

Builds the API image and starts it alongside Postgres, pgAdmin, Redis, MongoDB, Elasticsearch, and RabbitMQ.

```bash
docker compose -f API/docker-compose.yml up --build
```

Stop everything:

```bash
docker compose -f API/docker-compose.yml down
```

Wipe data volumes too (fresh start):

```bash
docker compose -f API/docker-compose.yml down -v
```

The API container reads its connection strings from `ASPNETCORE_*` / `ConfigSettings__*` env vars defined in the compose file, so it talks to the other containers by service name (`postgres`, `redis`, `mongo`, `elasticsearch`, `rabbitmq`).

---

## Option B — Infra in Docker, API from your IDE

Useful when you want the debugger attached.

1. Start only the infrastructure services:

   ```bash
   docker compose -f API/docker-compose.yml up -d postgres redis mongo elasticsearch rabbitmq
   ```

2. Apply EF Core migrations (or let the app do it on startup if `ConfigSettings.MigrationSettings.RunOnStartup` is `true`):

   ```bash
   dotnet ef --startup-project API --project DAL database update --context DataContext
   ```

3. Run the API:

   ```bash
   dotnet run --project API
   ```

The default `appsettings.Development.json` is already pointed at `localhost` for every dependency, so no edits required.

---

## Endpoints

| Service              | URL                                           | Notes                                          |
|----------------------|-----------------------------------------------|------------------------------------------------|
| API (Docker)         | http://localhost:8080/swagger                 | Swagger UI                                     |
| API (IDE)            | https://localhost:7086/swagger                | from `launchSettings.json`                     |
| pgAdmin              | http://localhost:8009                         | `admin@admin.com` / `postgres`                 |
| Postgres             | `localhost:5432`                              | `postgres` / `post123`, db `StarterAppDb`      |
| Redis                | `localhost:6379`                              |                                                |
| MongoDB              | `localhost:27017`                             |                                                |
| Elasticsearch        | http://localhost:9200                         | security disabled in dev                       |
| RabbitMQ AMQP        | `localhost:5672`                              | `guest` / `guest`                              |
| RabbitMQ Management  | http://localhost:15672                        | `guest` / `guest`                              |

Default seeded user for `requests.http`:

```
email:    test@test.tst
password: testtest
```

---

## Common tasks

**Tail the API logs:**

```bash
docker compose -f API/docker-compose.yml logs -f api
```

**Rebuild just the API after code changes:**

```bash
docker compose -f API/docker-compose.yml up --build api
```

**Drop just the API container (keep infra running):**

```bash
docker compose -f API/docker-compose.yml stop api
```

**Connect to Postgres via pgAdmin:** add a server with host `postgres` (the docker network name), port `5432`, user `postgres`, password `post123`.

---

## Trimming what you don't need

The compose file ships every optional dependency turned on. If your fork doesn't use one (e.g. Elasticsearch), delete the service block and its `volumes:` entry, and drop the matching extension wiring per the rule in [CLAUDE.md](CLAUDE.md): *keep what you use, delete what you don't*.
