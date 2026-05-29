namespace CORE.Config;

/// <summary>Swagger UI title / version / dark-theme stylesheet. Toggled by <see cref="Controllable.IsEnabled" />.</summary>
public record SwaggerSettings : Controllable
{
    public required string Title { get; set; }
    public required string Version { get; set; }
    public required string Theme { get; set; }
}