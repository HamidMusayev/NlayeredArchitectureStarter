using System.Reflection;
using BLL.Concrete;
using CORE.Config;
using DAL.EntityFramework.Context;
using DTO.User;
using ENTITIES.Entities;
using NetArchTest.Rules;

namespace TESTS.Architecture;

/// <summary>
///     Enforces the layering contract documented in <c>CLAUDE.md</c>. The rule of the codebase
///     is "one-way dependencies": <c>API → BLL → DAL</c>, with <c>CORE</c> referenced by everyone
///     and <c>ENTITIES</c> referenced only by <c>DAL</c> / <c>BLL</c>. These tests catch the slow
///     drift that turns a layered architecture into a ball of mud — failures here mean a PR
///     bypassed the convention and a reviewer didn't notice.
/// </summary>
public class LayerBoundaryTests
{
    // Marker types — picked because they're stable members of their respective assemblies.
    private static readonly Assembly EntitiesAsm = typeof(User).Assembly;
    private static readonly Assembly DtoAsm = typeof(UserToListDto).Assembly;
    private static readonly Assembly CoreAsm = typeof(ConfigSettings).Assembly;
    private static readonly Assembly DalAsm = typeof(DataContext).Assembly;
    private static readonly Assembly BllAsm = typeof(TokenService).Assembly;

    // In-house assembly *names* — for negative dependency rules ("must not reference any of these").
    private static readonly string[] InHouseAssemblies =
    [
        "API", "BLL", "DAL", "CORE", "ENTITIES", "DTO",
        "MEDIATRS", "GRAPHQL", "STORAGE", "MESSAGEBUS", "OUTBOX",
        "NOTIFICATIONS", "REFITS"
    ];

    [Fact]
    public void Entities_should_not_depend_on_any_other_in_house_project()
    {
        // ENTITIES is the leaf of the dependency tree — plain CLR types, framework-only.
        var forbidden = InHouseAssemblies.Where(a => a != "ENTITIES").ToArray();

        var result = Types.InAssembly(EntitiesAsm)
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        AssertConforms(result, "ENTITIES must reference framework code only.");
    }

    [Fact]
    public void Dto_should_not_depend_on_entity_types()
    {
        // DTOs are the boundary type for API/BLL. If they reference ENTITIES we've leaked the
        // persistence model into the wire shape — a refactor in the DB layer becomes a breaking
        // API change.
        // <para>
        //     Carve-out: <c>ENTITIES.Enums.*</c> are values (UserType, FileType) shared across all
        //     layers. They describe entity shape but they're not entity types — referencing them
        //     from DTOs doesn't couple the DTO to a persistence concern.
        // </para>
        var result = Types.InAssembly(DtoAsm)
            .Should()
            .NotHaveDependencyOnAny("ENTITIES.Entities", "ENTITIES.Entities.Generic")
            .GetResult();

        AssertConforms(result,
            "DTOs must not reference ENTITIES.Entities — only the ENTITIES.Enums kernel is fair game.");
    }

    [Fact]
    public void Core_should_not_depend_on_api_bll_or_dal()
    {
        // CORE holds cross-cutting abstractions consumed by every other project. Any reverse
        // dependency creates a cycle the moment one of those projects adds CORE back.
        var result = Types.InAssembly(CoreAsm)
            .Should()
            .NotHaveDependencyOnAny("API", "BLL", "DAL")
            .GetResult();

        AssertConforms(result, "CORE is a leaf abstraction layer — it must not reference API / BLL / DAL.");
    }

    [Fact]
    public void Bll_should_not_depend_on_api()
    {
        // BLL holds business rules; API holds HTTP shape. BLL must be HTTP-agnostic so the same
        // services can be reused from a different host (CLI tool, worker, GraphQL endpoint).
        var result = Types.InAssembly(BllAsm)
            .Should()
            .NotHaveDependencyOn("API")
            .GetResult();

        AssertConforms(result, "BLL must not reference API — keep business rules HTTP-agnostic.");
    }

    [Fact]
    public void Dal_should_not_depend_on_api()
    {
        // DAL is data access; API is HTTP. Same rationale as the BLL → API rule.
        var result = Types.InAssembly(DalAsm)
            .Should()
            .NotHaveDependencyOn("API")
            .GetResult();

        AssertConforms(result, "DAL must not reference API — keep persistence concerns separate from HTTP.");
    }

    [Fact]
    public void Bll_concrete_classes_should_not_depend_on_dal_concrete_classes()
    {
        // BLL injects DAL via interfaces (IFooRepository, IUnitOfWork), never concrete types.
        // This keeps swapping the DAL implementation (testing, alternate ORM) a non-invasive change.
        var result = Types.InAssembly(BllAsm)
            .That()
            .ResideInNamespaceStartingWith("BLL.Concrete")
            .Should()
            .NotHaveDependencyOn("DAL.EntityFramework.Concrete")
            .GetResult();

        AssertConforms(result, "BLL.Concrete must depend on DAL.EntityFramework.Abstract only, never on Concrete.");
    }

    private static void AssertConforms(TestResult result, string message)
    {
        if (result.IsSuccessful) return;

        var failed = string.Join(Environment.NewLine, result.FailingTypeNames ?? []);
        Assert.Fail($"{message}{Environment.NewLine}Offending types:{Environment.NewLine}{failed}");
    }
}