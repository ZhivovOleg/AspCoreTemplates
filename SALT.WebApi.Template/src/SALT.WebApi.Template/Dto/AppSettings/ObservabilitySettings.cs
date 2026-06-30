namespace SALT.WebApi.Template.Dto.AppSettings;

internal sealed class ObservabilitySettings
{
    public const string SectionName = "Observability";

    public string ServiceName { get; init; } = "SALT.WebApi.Template";

    public string ServiceNamespace { get; init; } = "SALT";

    public bool EnableOtlpExporter { get; init; } = true;

    public bool EnablePrometheusExporter { get; init; }

    public bool EnableConsoleExporter { get; init; }
}