using System.Collections.Generic;

namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Application settings.
/// </summary>
internal sealed class ApplicationSettings
{
    /// <summary>
    /// Logging settings.
    /// </summary>
    public LoggingSettings Logging { get; init; } = new();

    /// <summary>
    /// Web server setiings.
    /// </summary>
    public KestrelSettings Kestrel { get; init; } = new();

    /// <summary>
    /// Database connections.
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; init; } = [];

    /// <summary>
    /// Health checks settings.
    /// </summary>
    public HealthChecksSettings HealthChecks { get; init; } = new();

    /// <summary>
    /// Http reliability settings.
    /// </summary>
    public HttpPolicies HttpPolicies { get; init; } = new();
}
