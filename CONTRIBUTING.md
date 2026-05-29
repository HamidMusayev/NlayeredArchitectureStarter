# Contributing

Thanks for considering a contribution. This project is a starter template — the bar for changes is "would another team forking this be glad you made it?"

## Ground rules

1. **`dotnet build` must finish with 0 errors and no new warnings** before you open a PR.
2. **Don't break the layering**. See [Layer ownership](#layer-ownership). PRs that have a controller calling a repository directly will be sent back.
3. **Don't introduce a new top-level abstraction without a real second implementation in mind**. We've already trimmed the audit list once; let's not refill it.
4. **Code style follows the existing files** — primary constructors, `var` inside method bodies, no emoji in source. Let Rider's default formatter own the rest (brace placement, XML-doc indentation, expression-bodied vs block-bodied members, `using` imports vs fully-qualified type names). Don't hand-revert formatter output.

## Branch / PR model

- `main` is always shippable.
- Feature branches: `feat/<short-kebab>` (e.g. `feat/audit-log-table`).
- Fixes: `fix/<short-kebab>`.
- One topic per PR. If you find an unrelated cleanup, file it separately or include it in a "drive-by" PR clearly labelled as such.

PR description template:

```markdown
## What
1-3 bullet points

## Why
What problem this solves / what audit item it closes

## Verification
- [ ] `dotnet build` clean
- [ ] new/changed tests pass
- [ ] smoke-tested against the `requests.http` flow
```

## Commit messages

Plain imperative subject line under 70 chars. Body optional, used for the *why*.

```
fix: guard PermissionService.SoftDeleteAsync against missing id

NRE path closed; matches the pattern in RoleService/TokenService.
```

## Layer ownership

| Layer         | Owns                                          | Must NOT                                  |
|---------------|-----------------------------------------------|-------------------------------------------|
| `API`         | HTTP shape, auth filters, SignalR             | Talk to repositories directly             |
| `BLL`         | Business rules, orchestration, DTO mapping    | Return `IQueryable<T>`, return entities   |
| `DAL`         | EF + repositories, UnitOfWork                 | Reference HTTP / authentication concerns  |
| `CORE`        | Cross-cutting abstractions + config records   | Reference `BLL`, `DAL`, or `API`          |
| `ENTITIES`    | Plain entities, `Auditable` base              | Reference any other project               |
| `DTO`         | Records used at any boundary + validators     | Reference `ENTITIES`                      |
| `GRAPHQL`     | HotChocolate Query/Mutation/ObjectType<T>     | Hold business rules — delegate to BLL     |
| `MEDIATRS`    | CQRS handlers                                 | Become a dumping ground for everything    |
| `STORAGE`     | `IBlobStorage` contract + Filesystem/SFTP/S3 impls | Hold any non-storage abstraction      |

`IQueryable<T>` only crosses into the API surface from GraphQL via `IRoleRepository.GetList()`. The trade-off is documented in `GRAPHQL/Roles/Query.cs`.

## Adding a new endpoint (the well-trodden path)

Suppose you're adding `GET /api/Project/{id}`:

1. **Entity** — add `ENTITIES/Entities/Project.cs` extending `Auditable, IEntity`.
2. **DTO** — `DTO/Project/ProjectDtos.cs` with `ProjectToListDto`, `ProjectToAddDto`, etc.
3. **Validator** — `DTO/Project/Validators/ProjectToAddDtoValidator.cs` (FluentValidation).
4. **Mapper** — `BLL/Mappers/ProjectMapper.cs` (AutoMapper `Profile`).
5. **Repository** — `DAL/EntityFramework/Abstract/IProjectRepository.cs` + `DAL/EntityFramework/Concrete/ProjectRepository.cs` extending `GenericRepository<Project>`.
6. **Service** — `BLL/Abstract/IProjectService.cs` + `BLL/Concrete/ProjectService.cs`. Inject the repository directly, plus `IUnitOfWork` if you commit.
7. **Controller** — `API/Controllers/ProjectController.cs` extending `ControllerBase`, decorated `[ApiController]`, `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`, `[ValidateToken]`.
8. **Migration** — `dotnet ef --startup-project API --project DAL migrations add AddProject --context DataContext`.
9. **`requests.http`** — append a section so it's testable from the file.

DI is auto-wired: any `BLL.Concrete.*` and `DAL.EntityFramework.Concrete.*` class with a matching interface is picked up by Scrutor (`RegisterRepositories()` in `DependencyContainer.cs`).

For **CQRS-style** changes, mirror `MEDIATRS/OrganizationCQRS/*` instead.

## Adding a new transient utility

Goes in `CORE`:

- Interface in `CORE/Abstract/IXxxService.cs`
- Impl in `CORE/Concrete/XxxService.cs`
- Manual registration in `RegisterRepositories()` (Scrutor scans BLL/DAL only)
- Settings record under `CORE/Config/XxxSettings.cs` + property on `ConfigSettings`
- Sample values in both `appsettings.Development.json` and `appsettings.Production.json`

`IMailService`/`ISmsService` follow this exact pattern — copy them.

## Tests

- Controller-level unit tests live in `TESTS/` (xUnit + Moq). Mock `IXxxService`.
- Load tests live in `NBOOMERS/` (NBomber).

If you touch a public service contract, add or update a test in `TESTS/`.

## Things we don't want PRs for

- Renaming `Items` → `Data` or similar API-breaking cosmetic changes.
- Wholesale refactors that "improve" the architecture without a concrete problem.
- Adding a new MQ/cache/search vendor as a parallel example unless we drop something — the starter is already large.

## Questions

Open a GitHub Discussion, not an issue, for design questions. Issues are for bugs and concrete proposals.
