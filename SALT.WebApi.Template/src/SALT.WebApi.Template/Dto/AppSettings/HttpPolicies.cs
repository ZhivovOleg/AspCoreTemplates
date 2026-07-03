using System;

namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Http resilience politics.
/// <see href="https://learn.microsoft.com/en-us/dotnet/core/resilience"/>
/// </summary>
internal sealed class HttpPolicies
{
    public const string SectionName = "HttpPolicies";

    /// <summary>
    /// Total operation timeout, seconds.
    /// </summary>
    public TimeSpan OverallTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Single try tiomeout, seconds.
    /// </summary>
    public TimeSpan TimeoutPerTry { get; init; } = TimeSpan.FromSeconds(3);

    /// <summary>
    /// How many retries per operation.
    /// </summary>
    public int RetriesCount { get; init; } = 10;
}
