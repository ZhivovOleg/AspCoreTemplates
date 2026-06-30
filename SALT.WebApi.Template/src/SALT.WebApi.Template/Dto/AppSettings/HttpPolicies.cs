using System;

namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Http resilience politics.
/// <see href="https://learn.microsoft.com/en-us/dotnet/core/resilience"/>
/// </summary>
internal sealed class HttpPolicies
{
    /// <summary>
    /// Total operation timeout, seconds.
    /// </summary>
    public TimeSpan OverallTimeout { get; set; } = TimeSpan.FromSeconds(10);
    /// <summary>
    /// Single try tiomeout, seconds.
    /// </summary>
    public TimeSpan TimeoutPerTry { get; set; } = TimeSpan.FromSeconds(3);
    /// <summary>
    /// How many retries per operation.
    /// </summary>
    public int RetriesCount { get; set; } = 10;
}
