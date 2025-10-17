using System;

namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Http resilience politics.
/// <see href="https://learn.microsoft.com/en-us/dotnet/core/resilience"/>
/// </summary>
public class HttpPolicies
{
    /// <summary>
    /// Total operation timeout.
    /// </summary>
    public TimeSpan OverallTimeout { get; set; } = TimeSpan.FromSeconds(10);
    /// <summary>
    /// Single try tiomeout.
    /// </summary>
    public TimeSpan TimeoutPerTry { get; set; } = TimeSpan.FromSeconds(3);
    /// <summary>
    /// How many retries per operation.
    /// </summary>
    public int RetriesCount { get; set; } = 10;
}
