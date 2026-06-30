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
    public LoggingSettings Logging { get; set; }
    /// <summary>
    /// Web server setiings.
    /// </summary>
    public KestrelSettings Kestrel { get; set; }
    /// <summary>
    /// Database connections.
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; set; }
    /// <summary>
    /// Health checks settings.
    /// </summary>
    public HealthChecksSettings HealthChecks { get; set; }
    /// <summary>
    /// Http reliability settings.
    /// </summary>
    public HttpPolicies HttpPolicies { get; set; }
}