namespace AspCore.Microservices.Template.Dto.AppSettings;

/// <summary>
/// Health checks settings
/// </summary>
public class HealthChecksSettings
{
    /// <summary>
    /// Check interval in seconds
    /// </summary>
    public int IntervalSeconds { get; set; }
    /// <summary>
    /// Postgres connections
    /// </summary>
    public string[] PgSql { get; set; }
    /// <summary>
    /// TCP check
    /// </summary>
    public Host[] Tcp { get; set; }
}