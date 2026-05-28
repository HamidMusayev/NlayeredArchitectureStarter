# NLayered Architecture Starter

A pragmatic ASP.NET Core 10 starter you can fork to begin a real product. Clear N-layer split, JWT auth, EF Core + PostgreSQL, ready-to-go examples of MediatR/CQRS, GraphQL, Redis, MongoDB, Elasticsearch, Hangfire, SignalR, Refit, and more — every example is wired and runnable, but each one is opt-in via configuration.

---

## Quick start

```bash
git clone <your-fork-url>
cd NlayeredArchitectureStarter
# 1. point ConnectionStrings.AppDb at a Postgres instance (or run `docker compose up`)
# 2. run EF migrations
dotnet ef --startup-project API --project DAL database update --context DataContext
# 3. run the API
dotnet run --project API
```

Default URL: **https://localhost:7086** (Swagger opens automatically).

Try the API with the included [`requests.http`](./requests.http) — works in Rider, VS Code (REST Client extension) and Visual Studio. Open it, fire `Login`, and follow the chain.

Default seeded user:

```
email:    test@test.tst
password: testtest
```

---

## Stack

| Layer            | Tech                                                                |
|------------------|---------------------------------------------------------------------|
| API              | ASP.NET Core 10, JWT Bearer, Swagger, Hangfire, SignalR, GraphQL (HotChocolate) |
| Business         | MediatR (CQRS), FluentValidation, AutoMapper                        |
| Data             | EF Core 10 + Npgsql, Generic Repository, Unit of Work               |
| Optional         | Redis (Redis.OM), MongoDB, Elasticsearch, RabbitMQ-ready             |
| Infra utilities  | Refit HTTP client, MailKit, Twilio, SSH.NET (SFTP)                  |
| Observability    | Nummy code/exception/HTTP/health loggers, MiniProfiler              |

---

## Project layout

```
API/         Controllers, filters, GraphQL types, SignalR hubs, DI container, Program.cs
BLL/         Business services (interface + concrete), AutoMapper profiles, file-type handlers
CORE/        Cross-cutting: config records, abstractions (ICurrentUser, IJwtFactory, IPasswordHasher, IMailService, ISmsService, IEncryptionService, IPaginationContext, …), helpers
DAL/         EF Core context, generic + entity repositories, IUnitOfWork, ElasticSearch / MongoDB / Redis adapters
DTO/         Request/response records + FluentValidation validators
ENTITIES/    Domain entities, Auditable base, enums
MEDIATRS/    CQRS commands, queries, handlers (Organization example)
REFITS/      Refit HTTP client interfaces (ToDo example)
TESTS/       xUnit tests
NBOOMERS/    Load-test runner (NBomber)
```

### Layering rules

```
Controllers / GraphQL → BLL services → DAL repositories
                                    ↘ IUnitOfWork (transactions)
                       (no controller → repository shortcut)
```

`IQueryable<T>` never crosses the BLL boundary. The GraphQL `Query` is allowed to depend on repositories directly because it is itself a read projection.

---

## Built-in features

- **Auth** — JWT bearer, refresh tokens, custom `[ValidateToken]` action filter, password reset via OTP
- **AuthZ** — Hangfire dashboard + GraphQL endpoint both require a valid JWT
- **Auditing** — every `Auditable` entity gets `CreatedAt`, `CreatedById`, `ModifiedAt`, `ModifiedBy`, `DeletedAt`, `DeletedBy`, `IsDeleted` filled automatically from `ICurrentUser`
- **Soft delete** — global query filter on `IsDeleted` + `repository.SoftDelete(entity)`
- **Encryption** — `IEncryptionService` (AES) used for user-id JWT claim
- **Password hashing** — `IPasswordHasher` (PBKDF2-SHA512, 100k iters, per-user salt)
- **Email** — `IMailService` via MailKit
- **SMS** — `ISmsService` via Twilio
- **File uploads** — type-aware policy (per-`FileType` validators + max size), JS-stripping for PDFs available via `FileHelper`
- **CQRS** — `MEDIATRS/OrganizationCQRS` is a complete CRUD slice you can copy
- **GraphQL** — `Role` query with projection / sorting / filtering pushed down to SQL; audit columns hidden via `ObjectType<>`
- **Background jobs** — Hangfire with PostgreSQL storage; sample recurring `CounterJob`
- **Health check** — `/nummy/health`
- **Rate limit** — fixed-window per-user, 5 req / 10 s (configurable in `RegisterRateLimit`)
- **Localization** — `lang` header switches `MsgResource` translations (az / en / ru)

---

## Configuration

Settings live in `appsettings.Development.json` and `appsettings.Production.json` under the `ConfigSettings` root. Strongly-typed via `CORE/Config/ConfigSettings.cs`.

Toggle subsystems with the `IsEnabled` flags:

```jsonc
"RedisSettings":         { "IsEnabled": false, ... },
"ElasticSearchSettings": { "IsEnabled": false, ... },
"MongoDbSettings":       { "IsEnabled": true,  ... },
"SwaggerSettings":       { "IsEnabled": true,  ... }
```

> Secrets (`AuthSettings.SecretKey`, `CryptographySettings.*`, `TwilioSettings.AuthToken`, mail/SFTP passwords) ship with placeholder values. Move them to **User Secrets** (`dotnet user-secrets`) or your platform's secret store before deployment.

---

## Dev URLs

| Endpoint           | Path                  | Auth |
|--------------------|-----------------------|------|
| Swagger UI         | `/swagger`            | open |
| GraphQL            | `/graphql`            | JWT  |
| GraphQL Voyager    | `/graphql-voyager`    | open |
| Hangfire dashboard | `/api/hangfire`       | JWT  |
| Health check       | `/nummy/health`       | open |
| SignalR hub        | `/userHub`            | JWT  |

---

## Tests

```bash
dotnet test
```

The `TESTS` project has controller-level examples using xUnit + Moq. The `NBOOMERS` project is a separate NBomber-based load runner.

---

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md).

---

## License

MIT.
