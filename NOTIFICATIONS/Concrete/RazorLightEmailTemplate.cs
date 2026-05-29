using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using NOTIFICATIONS.Abstract;
using RazorLight;

namespace NOTIFICATIONS.Concrete;

/// <summary>
///     Default <see cref="IEmailTemplate" /> implementation — Razor-rendered <c>.cshtml</c>
///     files under <c>{ContentRoot}/Templates/Email/</c>. Templates are compiled on first
///     use and cached for the process lifetime.
///     <para>
///         Subject convention: the first <c>@*subject: ...*@</c> Razor comment block in the
///         rendered output becomes the email subject and is stripped from the body. Lets
///         localized subject lines travel with their template.
///     </para>
/// </summary>
public sealed partial class RazorLightEmailTemplate : IEmailTemplate
{
    private readonly RazorLightEngine _engine;

    public RazorLightEmailTemplate(IHostEnvironment env)
    {
        var root = Path.Combine(env.ContentRootPath, "Templates", "Email");
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(root)
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<RenderedEmail> RenderAsync<TModel>(string templateName, TModel model,
        CancellationToken ct = default)
    {
        var rendered = await _engine.CompileRenderAsync(templateName, model);

        var subject = "(no subject)";
        var subjectMatch = SubjectRegex().Match(rendered);
        if (subjectMatch.Success)
        {
            subject = subjectMatch.Groups[1].Value.Trim();
            rendered = rendered.Replace(subjectMatch.Value, string.Empty).TrimStart();
        }

        return new RenderedEmail(subject, rendered);
    }

    [GeneratedRegex(@"@\*\s*subject:\s*(.+?)\s*\*@", RegexOptions.Singleline)]
    private static partial Regex SubjectRegex();
}