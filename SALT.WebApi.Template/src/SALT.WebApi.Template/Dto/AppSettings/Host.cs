namespace SALT.WebApi.Template.Dto.AppSettings;

/// <summary>
/// Host for TCP check 
/// </summary>
internal sealed class Host
{
    /// <summary>
    /// Full name or IP
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Port
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Short description
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
