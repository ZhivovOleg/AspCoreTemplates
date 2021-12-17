using System.Collections.Generic;

namespace AspCore.Microservices.Template.Dto.AppSettings;

/// <summary>
/// Application settings 
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Logging settings
    /// </summary>
    public LoggingSettings Logging { get; set; }
    /// <summary>
    /// Web server setiings
    /// </summary>
    public KestrelSettings Kestrel { get; set; }
    /// <summary>
    /// Database connections 
    /// </summary>
    public Dictionary<string, string> Connections { get; set; }
    /// <summary>
    /// Health checks settings
    /// </summary>
    public HealthChecksSettings HealthChecks { get; set; }
}