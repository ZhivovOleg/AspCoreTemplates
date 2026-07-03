namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Health checks settings
/// </summary>
internal sealed class HealthChecksSettings
{
    /// <summary>
    /// Check interval in seconds
    /// </summary>
    public int IntervalSeconds { get; init; } = 60;

    /// <summary>
    /// Postgres connections
    /// </summary>
    public string[] PgSql { get; init; } = [];

    /// <summary>
    /// TCP check
    /// </summary>
    public Host[] Tcp { get; init; } = [];
}
