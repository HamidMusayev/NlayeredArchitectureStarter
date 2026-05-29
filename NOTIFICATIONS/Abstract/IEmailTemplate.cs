namespace NOTIFICATIONS.Abstract;

/// <summary>
///     Renders an email subject + body from a named template and a strongly-typed model.
///     Default implementation (<c>RazorLightEmailTemplate</c>) reads <c>.cshtml</c> files
///     from disk; tests can swap in a stub that returns canned output.
///     <para>
///         Subject lines live in the template file's first non-empty line prefixed with
///         <c>@*subject:*@ ...</c>; everything after that is the body. Keeps subject + body
///         versioned together rather than spread across config and code.
///     </para>
/// </summary>
public interface IEmailTemplate
{
    Task<RenderedEmail> RenderAsync<TModel>(string templateName, TModel model, CancellationToken ct = default);
}

/// <summary>
///     Rendered output of an <see cref="IEmailTemplate" /> call.
/// </summary>
public sealed record RenderedEmail(string Subject, string Body);