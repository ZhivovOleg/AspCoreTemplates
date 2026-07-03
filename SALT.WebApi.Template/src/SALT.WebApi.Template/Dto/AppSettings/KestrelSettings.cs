namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Web server settings
/// </summary>
internal sealed class KestrelSettings
{
    public const string SectionName = "Kestrel";

    /// <summary>
    /// Application listen on
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Keep alive timeout in seconds.
    /// </summary>
    public int KeepAliveTimeout { get; init; } = 60;

    /// <summary>
    /// Request headers timeout in seconds.
    /// </summary>
    public int RequestHeadersTimeout { get; init; } = 60;

    /// <summary>
    /// Add or no ForwardedHeaders 
    /// </summary>
    public bool UseReverseProxy { get; init; }
}
