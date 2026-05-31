using System.Reflection;
using BLL.Abstract;
using ENTITIES.Entities;

namespace TESTS.Architecture;

/// <summary>
///     Enforces the documented "BLL must not return <c>IQueryable&lt;T&gt;</c>" rule from
///     <c>CLAUDE.md</c>. Returning <c>IQueryable</c> from a service punches a hole through the
///     business layer — the caller (controller, GraphQL field) can now compose arbitrary SQL,
///     defer execution past the using-scope of the DbContext, or attach predicates that bypass
///     business rules.
///     <para>
///         The one documented exception lives at <c>IRoleRepository.GetList()</c> in the DAL —
///         <em>not</em> in BLL — so this rule scoped to <c>BLL.Abstract</c> won't trip on it. If a
///         future exception is justified, add it to the <c>AllowedExceptions</c> set below with a
///         comment explaining why.
///     </para>
///     <para>
///         Also blocks BLL from returning raw <c>ENTITIES</c> types — handlers should map to
///         DTOs at the BLL boundary so the persistence model doesn't bleed into HTTP.
///     </para>
/// </summary>
public class QueryableLeakTests
{
    private static readonly Assembly BllAbstractAsm = typeof(ITokenService).Assembly;
    private static readonly Assembly EntitiesAsm = typeof(User).Assembly;

    /// <summary>
    ///     Add a fully-qualified method name (e.g. <c>"BLL.Abstract.IFoo.GetQueryable"</c>) only
    ///     with a comment justifying the deviation.
    /// </summary>
    private static readonly HashSet<string> AllowedExceptions = new(StringComparer.Ordinal);

    [Fact]
    public void Bll_abstract_methods_must_not_return_iqueryable()
    {
        var offenders = BllAbstractAsm.GetTypes()
            .Where(t => t.IsInterface && t.Namespace?.StartsWith("BLL.Abstract", StringComparison.Ordinal) == true)
            .SelectMany(t => t.GetMethods())
            .Where(m => IsQueryable(m.ReturnType))
            .Where(m => !AllowedExceptions.Contains($"{m.DeclaringType!.FullName}.{m.Name}"))
            .Select(m => $"{m.DeclaringType!.FullName}.{m.Name} → {m.ReturnType}")
            .ToList();

        if (offenders.Count == 0) return;

        Assert.Fail(
            "BLL.Abstract methods must not expose IQueryable. Project the data and return DTOs/lists instead." +
            Environment.NewLine + "Offenders:" + Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    [Fact]
    public void Bll_abstract_methods_must_not_return_entities()
    {
        var offenders = BllAbstractAsm.GetTypes()
            .Where(t => t.IsInterface && t.Namespace?.StartsWith("BLL.Abstract", StringComparison.Ordinal) == true)
            .SelectMany(t => t.GetMethods())
            .Where(m => LeaksEntity(m.ReturnType))
            .Select(m => $"{m.DeclaringType!.FullName}.{m.Name} → {m.ReturnType}")
            .ToList();

        if (offenders.Count == 0) return;

        Assert.Fail(
            "BLL.Abstract methods must not return ENTITIES types — map to DTOs at the BLL boundary." +
            Environment.NewLine + "Offenders:" + Environment.NewLine +
            string.Join(Environment.NewLine, offenders));
    }

    private static bool IsQueryable(Type type)
    {
        if (type == typeof(IQueryable)) return true;
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IQueryable<>)) return true;
        // Unwrap Task<T> / ValueTask<T> / IResult-wrapped generics.
        if (type.IsGenericType)
            return type.GetGenericArguments().Any(IsQueryable);
        return false;
    }

    private static bool LeaksEntity(Type type)
    {
        // Carve-out: ENTITIES.Enums are values shared across every layer (UserType, FileType,
        // ...) — referencing them from BLL.Abstract is fine. The rule is about entity types.
        if (type.Assembly == EntitiesAsm &&
            type.Namespace?.StartsWith("ENTITIES.Enums", StringComparison.Ordinal) != true)
            return true;
        if (type.IsGenericType)
            return type.GetGenericArguments().Any(LeaksEntity);
        return false;
    }
}