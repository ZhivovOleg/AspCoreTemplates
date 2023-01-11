namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Web server settings
/// </summary>
public class KestrelSettings
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