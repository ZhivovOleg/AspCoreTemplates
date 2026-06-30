namespace SALT.WebApi.Template.Dto.AppSettings;

internal sealed class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; init; } = [];
}