namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Web server settings
/// </summary>
internal sealed class KestrelSettings
{
    /// <summary>
    /// Application listen on
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// Add or no ForwardedHeaders 
    /// </summary>
    public bool UseReverseProxy { get; set; }
}