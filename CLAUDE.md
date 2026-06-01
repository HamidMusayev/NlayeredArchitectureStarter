# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build (must finish with 0 errors / no new warnings before opening a PR)
dotnet build

# Run the API (Swagger at https://localhost:7086/swagger)
dotnet run --project API

# EF Core migrations — DAL is the migrations project, API is the startup
dotnet ef --startup-project API --project DAL migrations add <Name> --context DataContext
dotnet ef --startup-project API --project DAL database update --context DataContext

# Tests
dotnet test                                  # all
dotnet test TESTS/TESTS.csproj               # just the unit-test project
dotnet test --filter "FullyQualifiedName~RoleControllerTests"   # single class
dotnet test --filter "DisplayName~Login_ReturnsToken"           # single test

# Load tests (separate NBomber runner, not part of `dotnet test`)
dotnet run --project NBOOMERS -c Release
```

Default seeded user for `requests.http` flow: `test@test.tst` / `testtest`.

`global.json` pins `rollForward: latestMajor` with `allowPrerelease`; the projects target **net10.0**.

## Architecture

Strict N-layer with one-way dependencies. The layering is **enforced by review** — do not bypass it.

```
API → BLL → DAL          CORE is referenced by everyone
          ↘ IUnitOfWork  ENTITIES referenced by DAL/BLL only
          ↘ CORE          DTO referenced by API/BLL (never ENTITIES)
```

| Layer       | Owns                                          | Must NOT                                  |
|-------------|-----------------------------------------------|-------------------------------------------|
| `API`       | HTTP shape, filters, SignalR, DI bootstrap    | Talk to repositories directly             |
| `BLL`       | Business rules, orchestration, DTO mapping    | Return `IQueryable<T>` or entities        |
| `DAL`       | EF Core, repositories, `IUnitOfWork`          | Reference HTTP / auth concerns            |
| `CORE`      | Cross-cutting abstractions + `ConfigSettings` records | Reference BLL/DAL/API             |
| `ENTITIES`  | Plain entities, `Auditable` base              | Reference any other project               |
| `DTO`       | Records at boundaries + FluentValidation      | Reference `ENTITIES.Entities` (the `ENTITIES.Enums` kernel is OK) |
| `GRAPHQL`   | HotChocolate `Query`/`Mutation`/`ObjectType<T>` (e.g. `Roles`) | Hold business rules — delegate to BLL |
| `MEDIATRS`  | CQRS handlers (e.g. `OrganizationCQRS`)       | —                                         |
| `STORAGE`   | `IBlobStorage` contract + Filesystem/SFTP/S3 impls | Hold any non-storage abstraction     |

**The one documented exception:** `IRoleRepository.GetList()` returns `IQueryable<T>` that crosses into `GRAPHQL/Roles/Query.cs` so HotChocolate can push projection/sorting/filtering to SQL. Don't add new exceptions casually.

The layering is **also enforced by tests**: see [TESTS/Architecture/](TESTS/Architecture/). `LayerBoundaryTests` (NetArchTest) covers project-level dependency rules; `QueryableLeakTests` (reflection) covers the `BLL.Abstract` method-return-type rules. CI fails if a PR drifts.

### DI registration

Scrutor auto-scans `BLL.Concrete.*` and `DAL.EntityFramework.Concrete.*` and binds each class to its matching interface (`RegisterRepositories()` in the API project's container bootstrap). **CORE services are NOT auto-scanned** — register them manually in `RegisterRepositories()`. Use `IMailService`/`ISmsService` as templates when adding a CORE utility.

### Cross-cutting conventions

- **Options pattern, not the aggregate:** every per-feature settings record under `CORE.Config` is bound into DI via `services.AddConfigOptions(builder.Configuration)` (see `CoreServicesExtensions`). Runtime services depend on the narrow `IOptions<XSettings>` they actually need (e.g. `IOptions<AuthSettings>`) — **never** inject the whole `ConfigSettings` aggregate. The aggregate type still exists in `CORE.Config` for documentation purposes but is not registered in DI; asking for it will fail. The startup extensions in `API/Extensions/*.cs` use the helper `configuration.GetConfigSection<XSettings>()` for the rare branch (choosing which provider to register) that needs to inspect a value before any DI scope exists.
- **Auditing:** every entity that extends `Auditable` gets `CreatedAt/CreatedById/ModifiedAt/ModifiedBy/DeletedAt/DeletedBy/IsDeleted` + `TenantId` filled automatically by `AuditableInterceptor` (registered against the `DbContext` in `EntityFrameworkExtensions`). Don't set these fields manually.
- **Soft delete:** global query filter on `IsDeleted`. `SoftDeleteInterceptor` rewrites any `Remove()` on an `Auditable` entity into a soft delete transparently — `repository.SoftDelete(entity)` and `repository.Remove(entity)` are now equivalent. To include deleted rows, use `IgnoreQueryFilters()` explicitly. Interceptor order is fixed: SoftDelete runs first (state flip), then Auditable stamps `DeletedAt/DeletedBy`.
- **Auth:** controllers use `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` + the custom `[ValidateToken]` action filter. The filter extracts the JWT's `jti` claim, looks it up via `ITokenIntrospectionCache` (DB on miss), and short-circuits with 401 if the row is missing or revoked. The full JWT is never persisted — only `Token.Jti` lands in the database. Hangfire dashboard and `/graphql` are also JWT-gated.
- **Access vs refresh lifetimes:** access tokens default to **15 minutes** (`AuthSettings.AccessTokenLifetimeMinutes`) so a leaked JWT has a tight blast radius. Refresh tokens default to **14 days** (`AuthSettings.RefreshTokenLifetimeMinutes`) as an absolute window — clients call `GET /api/Auth/refresh` to rotate, and reuse-detection burns the family on replay. SPAs / mobile clients need a 401 → refresh → retry interceptor to keep this transparent.
- **JWT user-id claim** is AES-encrypted via `IEncryptionService` — read it through `ICurrentUser`, not by parsing `HttpContext.User` directly.
- **Passwords:** `IPasswordHasher` = PBKDF2-SHA512, 100k iterations, per-user salt. Never hash/verify inline.
- **Account lockout:** `AuthService.LoginAsync` increments `User.FailedLoginAttempts` on every wrong-password attempt and sets `User.LockedUntil = now + LockoutDurationMinutes` once attempts reach `MaxFailedLoginAttempts` (defaults 5 / 30 min, see `AuthSettings`). Successful login clears both. The lockout, "unknown email," and "wrong password" branches all return the same generic `InvalidUserCredentials` message so probes can't distinguish them.
- **Audit log:** `IAuditLog.LogAsync(action, …)` records sensitive events to the `AuditLog` table. The implementation uses an isolated `IServiceScope`, so the audit row commits independently of the caller's `IUnitOfWork` — failed writes are caught and logged. Action names are dotted lowercase (`auth.login.success`, `auth.password.reset`). Rows past `AuditLogSettings.RetentionDays` (default 365) are bulk-deleted by the `AuditLogPruneJob` recurring Hangfire job at 02:30 UTC daily.
- **Outbox dead-letter:** `OutboxDispatcherHostedService` increments `OutboxMessage.AttemptCount` on every tick that touches a row and sets `DeadLetteredAt` once `MessageBusSettings.OutboxMaxAttempts` (default 10) is reached. Dead-lettered rows are excluded from `GetPendingAsync` so a poison message can't block the queue head. Inspect / resurrect via `GET /api/OutboxAdmin/dead-letter` and `POST /api/OutboxAdmin/dead-letter/{id}/resurrect` (JWT + `[ValidateToken]`).
- **Outbox cleanup:** `OutboxCleanupJob` runs daily at 02:45 UTC and bulk-deletes processed rows older than `MessageBusSettings.OutboxRetentionDays` (default 30). Dead-lettered rows are spared — those need triage, not cleanup.
- **User permission cache:** `IUserPermissionsCache.GetAsync(userId)` returns the user's permission `Key` set (DB on miss, cached for `CacheSettings.UserPermissionsTtlMinutes`). Authorization filters / policy handlers should resolve permissions through this — never query directly. Invalidate via `InvalidateAsync(userId)` from any write that affects the answer: `UserService.UpdateAsync` (single user), `RoleService.UpdateAsync` / `SoftDeleteAsync` (every user holding the role, fanned via `IUserRepository.GetUserIdsByRoleAsync`).
- **Compiled EF queries on hot paths:** repository queries that run per-request (`TokenRepository.IsValid`, `TokenRepository.GetForValidationAsync`, `UserRepository.GetUserSaltAsync`, `UserRepository.GetPermissionKeysAsync`) are wrapped in `EF.CompileAsyncQuery` so query translation happens once at type init. When you add a method that runs at request rate, follow the same pattern: hoist the `EF.CompileAsyncQuery(...)` into a `private static readonly Func<...>` field, project to scalars or DTOs to avoid hydrating entities, and use the streaming `IAsyncEnumerable<T>` overload for list results. Skip compilation for write-rare admin queries — the win doesn't justify the boilerplate there.
- **Pagination contract:** list endpoints return `DTO.Common.PagedResult<T>` (`Items`, `Page`, `PageSize`, `Total` plus computed `HasNext` / `HasPrevious` / `TotalPages`). Build it in BLL via `IQueryable<T>.PageAsync(page, pageSize)` from `DAL.EntityFramework.Utility` — always page off an `OrderBy`-ed query or EF will warn. Use `PagedResult<T>.Map(...)` to project entities → DTOs while preserving the metadata. `UserService.GetAsPaginatedListAsync` is the canonical example.
- **Mappers (Mapperly only):** every mapper under `BLL.Mappers` is a `[Mapper] partial class` source-generated at build time by Riok.Mapperly. Inject the specific mapper class (`UserMapper`, `RoleMapper`, …) — there is no `IMapper`. Pattern: outbound `ToListDto(entity)` / `ToListDtos(IEnumerable<entity>)` returns DTOs; inbound uses `UpdateEntity(dto, target)` on a caller-constructed or loaded entity so `required` fields the DTO doesn't carry stay untouched. For `MapFrom` rules in the old AutoMapper Profiles, use `[MapProperty(nameof(src.Foo), nameof(dst.Bar))]`; for `.Ignore()`, use `[MapperIgnoreTarget]` / `[MapperIgnoreSource]`. All mappers register as singletons in `BusinessServicesExtensions` since they're stateless.
- **Strongly-typed identifiers:** id types are `public readonly record struct FooId(Guid Value)` under `ENTITIES.Identifiers`, decorated with `[JsonConverter(typeof(FooIdJsonConverter))]` + `[TypeConverter(typeof(FooIdTypeConverter))]` so wire / route shapes stay plain Guid. Pair each with a `ValueConverter<FooId, Guid>` under `DAL.EntityFramework.Conversions`, then register globally via `configurationBuilder.Properties<FooId>().HaveConversion<FooIdValueConverter>()` in `DataContext.ConfigureConventions`. Currently in use: `TenantId`, `RoleId`, `OrganizationId`, `UserId`. Convention: type **foreign keys and service-level parameters**, leave the entity's own `Id` as the inherited `Auditable.Id` Guid (avoids restructuring the base class). When you need a raw Guid (EF predicate, JWT claim assembly), use `.Value`; when you have a raw Guid and need the typed wrapper, use `new FooId(g)`. The compiler catches the mistakes that matter — passing a `UserId` to a method expecting `RoleId` is a build error.
- **Correlation id end-to-end:** `CorrelationIdMiddleware` reads / generates `X-Correlation-Id`, pushes it onto `Activity.Current` baggage (key `CorrelationContext.Key`) and Serilog's `LogContext`. Cross-thread async paths capture it via `CORE.Concrete.Observability.CorrelationContext.Current` and restore it on the consumer side via `CorrelationContext.Push(id)` inside a `using`: outbox rows carry `OutboxMessage.CorrelationId` (captured at enqueue, restored before handler invocation in `OutboxDispatcherHostedService`); in-process message-bus envelopes carry `MessageEnvelope.CorrelationId`; RabbitMQ uses the broker-native `BasicProperties.CorrelationId` slot; the background-task queue wraps the work item with a captured snapshot. Outbound Refit HTTP calls carry the header via `CorrelationIdDelegatingHandler`. The result: log lines from a request → outbox handler → message-bus consumer → outbound HTTP call all share the same id.
- **Refresh tokens at rest:** stored as SHA-256 hex digest only (`Token.RefreshTokenHash`). The plaintext is returned once on login in the response DTO, held by the client, and sent back on rotation — never persisted. Rotation reads via `ITokenRepository.GetByRefreshTokenHashAsync(TokenIntrospectionCache.Hash(plaintext))`. Use `TokenIntrospectionCache.Hash` whenever you need to compute the hash; never invent a parallel hasher.
- **Localization:** `lang` header (`az` / `en` / `ru`) drives `MsgResource` translations via `LocalizationMiddleware`.
- **Rate limit:** two layers. Global fixed-window per-user 5 req / 10 s applies to every request. A separate named policy `"auth"` (10 req / 5 min, partitioned per remote IP) applies to pre-auth endpoints — `login`, `refresh`, `otp`, `password/reset` — to slow credential stuffing. Apply with `[EnableRateLimiting("auth")]`; configured in `RateLimitExtensions`.

### Optional subsystems

Only `SwaggerSettings` ships with an `IsEnabled` flag — code paths must remain runnable with Swagger disabled, so guard its registrations on the flag.

Every other subsystem (EF Core, MongoDB, Elasticsearch, Redis, multi-tenancy, OpenTelemetry, ...) is **not** gated by config. Each has its own extension class under `API/Extensions/` and is wired unconditionally in [Program.cs](API/Program.cs). If a derived project does not need one, delete the extension file, its settings record, the appsettings block, and the call in `Program.cs`. The rule is **keep what you use, delete what you don't** — no dead flags. Multi-tenancy specifically is pinned to `ClaimsTenantResolver`; single-tenant projects should remove `AddMultiTenancy`, `ITenant`/`ClaimsTenantResolver`, and the `TenantId` column on `Auditable`.

### Pluggable blob storage

`IBlobStorage` (in [STORAGE](STORAGE), namespace `STORAGE.Abstract`) is the vendor-neutral byte-storage contract. Three impls live under `STORAGE.Concrete`: `FilesystemBlobStorage`, `SftpBlobStorage`, `S3BlobStorage` (MinIO-compatible). Pick one at startup via `BlobStorageSettings.Provider`; wiring in [BlobStorageExtensions.cs](API/Containers/Extensions/BlobStorageExtensions.cs). Add a new backend by dropping another class into `STORAGE.Concrete` and a new enum branch into the extension — no other code change.

The data layer does **not** use this pattern: persistence stays on EF Core + `IGenericRepository<T>` + `IUnitOfWork`. Mongo and Redis adapters in `DAL/MongoDb` and `DAL/Redis` are demos for those specific stores, not provider-switched backends for the relational entities.

### Adding a feature — the well-trodden path

For a typical CRUD endpoint, mirror an existing slice:

1. `ENTITIES/Entities/Foo.cs` extending `Auditable, IEntity`
2. `DTO/Foo/FooDtos.cs` + `DTO/Foo/Validators/FooToAddDtoValidator.cs`
3. `BLL/Mappers/FooMapper.cs` (AutoMapper `Profile`)
4. `DAL/EntityFramework/Abstract/IFooRepository.cs` + concrete extending `GenericRepository<Foo>`
5. `BLL/Abstract/IFooService.cs` + concrete; inject `IUnitOfWork` if you commit
6. `API/Controllers/FooController.cs` with `[ApiController]`, `[Authorize(...)]`, `[ValidateToken]`
7. EF migration (`dotnet ef ... migrations add AddFoo --context DataContext`)
8. Append a section to `requests.http`

For command/query style, copy `MEDIATRS/OrganizationCQRS/*` instead.

## Style

Match existing files: primary constructors, `var` inside methods, no emoji in source. Don't fight Rider's default formatter — it owns brace placement, XML-doc indentation, and the expression-bodied-vs-block-bodied choice. `using` imports with short type names are fine; reach for fully-qualified names only when an import would be ambiguous.

## Docs are part of the change

Whenever a code change affects anything documented in [README.md](README.md), [CONTRIBUTING.md](CONTRIBUTING.md), or this file, update those docs in the same change. Specifically: adding/removing/renaming a project, shifting layer responsibilities, changing commands (build/test/migration), changing the feature-slice recipe, or changing cross-cutting conventions. No follow-up "docs PR" — the docs ship with the code.
