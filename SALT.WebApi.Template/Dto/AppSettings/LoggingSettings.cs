using System.Collections.Generic;

namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Logging settings
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Levels for loggers
    /// </summary>
    public Dictionary<string, string> LogLevel { get; set; }
    /// <summary>
    /// Advanced exceptions handling settings
    /// </summary>
    public AdvancedExceptionsHandlingSettings AdvancedExceptionsHandling { get; set; }
}