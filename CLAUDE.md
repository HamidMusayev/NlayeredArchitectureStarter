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
| `DTO`       | Records at boundaries + FluentValidation      | Reference `ENTITIES`                      |
| `GRAPHQL`   | HotChocolate `Query`/`Mutation`/`ObjectType<T>` (e.g. `Roles`) | Hold business rules — delegate to BLL |
| `MEDIATRS`  | CQRS handlers (e.g. `OrganizationCQRS`)       | —                                         |
| `STORAGE`   | `IBlobStorage` contract + Filesystem/SFTP/S3 impls | Hold any non-storage abstraction     |

**The one documented exception:** `IRoleRepository.GetList()` returns `IQueryable<T>` that crosses into `GRAPHQL/Roles/Query.cs` so HotChocolate can push projection/sorting/filtering to SQL. Don't add new exceptions casually.

### DI registration

Scrutor auto-scans `BLL.Concrete.*` and `DAL.EntityFramework.Concrete.*` and binds each class to its matching interface (`RegisterRepositories()` in the API project's container bootstrap). **CORE services are NOT auto-scanned** — register them manually in `RegisterRepositories()`. Use `IMailService`/`ISmsService` as templates when adding a CORE utility.

### Cross-cutting conventions

- **Auditing:** every entity that extends `Auditable` gets `CreatedAt/CreatedById/ModifiedAt/ModifiedBy/DeletedAt/DeletedBy/IsDeleted` filled automatically from `ICurrentUser` during `SaveChanges`. Don't set these fields manually.
- **Soft delete:** global query filter on `IsDeleted`. Use `repository.SoftDelete(entity)`, not `Remove`. To include deleted rows, use `IgnoreQueryFilters()` explicitly.
- **Auth:** controllers use `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]` + the custom `[ValidateToken]` action filter (checks the token is still active server-side, not just signed). Hangfire dashboard and `/graphql` are also JWT-gated.
- **JWT user-id claim** is AES-encrypted via `IEncryptionService` — read it through `ICurrentUser`, not by parsing `HttpContext.User` directly.
- **Passwords:** `IPasswordHasher` = PBKDF2-SHA512, 100k iterations, per-user salt. Never hash/verify inline.
- **Localization:** `lang` header (`az` / `en` / `ru`) drives `MsgResource` translations via `LocalizationMiddleware`.
- **Rate limit:** fixed-window per-user 5 req/10s, configured in `RegisterRateLimit`.

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
