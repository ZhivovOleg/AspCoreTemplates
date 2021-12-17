namespace AspCore.Microservices.Template.Dto.AppSettings;

/// <summary>
/// Host for TCP check 
/// </summary>
public class Host
{
    /// <summary>
    /// Full name or IP
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Port
    /// </summary>
    public int Port { get; set; }
    /// <summary>
    /// Short description
    /// </summary>
    public string Descsription { get; set; }
}