using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using SALT.WebApi.Template.AppCore;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Dto.AppSettings;
using SALT.WebApi.Template.Net;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.Extensions;

/// <summary>
/// Extensions for ServiceCollection
/// </summary>
internal static class ServiceCollectionExtensions
{
    internal const string _corsPolicyName = "DefaultCorsPolicy";

    /// <summary>
    /// Add logic 
    /// </summary>
    internal static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfigurationRoot configuration)
    {
        _ = services.Configure<ApplicationSettings>(configuration);
        _ = services.AddTransient<IExampleDatabaseService, ExampleDatabaseService>();
        _ = services.AddTransient<IExampleLogicService, ExampleLogicService>();
        return services;
    }

    internal static IServiceCollection AddConfigurationOptions(this IServiceCollection services)
    {
        _ = services
            .AddOptions<ApplicationSettings>()
            .BindConfiguration(string.Empty)
            .ValidateOnStart();

        _ = services
            .AddOptions<KestrelSettings>()
            .BindConfiguration(KestrelSettings.SectionName)
            .Validate(settings => settings.Port > 0, "Kestrel:Port must be greater than zero.")
            .Validate(settings => settings.KeepAliveTimeout > 0, "Kestrel:KeepAliveTimeout must be greater than zero.")
            .Validate(settings => settings.RequestHeadersTimeout > 0, "Kestrel:RequestHeadersTimeout must be greater than zero.")
            .ValidateOnStart();

        _ = services
            .AddOptions<CorsSettings>()
            .BindConfiguration(CorsSettings.SectionName)
            .Validate(
                settings => settings.AllowedOrigins.All(static origin => Uri.IsWellFormedUriString(origin, UriKind.Absolute)),
                "Cors:AllowedOrigins must contain absolute URLs.")
            .ValidateOnStart();

        _ = services
            .AddOptions<HttpPolicies>()
            .BindConfiguration(HttpPolicies.SectionName)
            .Validate(settings => settings.OverallTimeout > TimeSpan.Zero, "HttpPolicies:OverallTimeout must be greater than zero.")
            .Validate(settings => settings.TimeoutPerTry > TimeSpan.Zero, "HttpPolicies:TimeoutPerTry must be greater than zero.")
            .Validate(settings => settings.TimeoutPerTry <= settings.OverallTimeout, "HttpPolicies:TimeoutPerTry must not exceed OverallTimeout.")
            .Validate(settings => settings.RetriesCount >= 0, "HttpPolicies:RetriesCount must be greater than or equal to zero.")
            .ValidateOnStart();

        _ = services
            .AddOptions<ObservabilitySettings>()
            .BindConfiguration(ObservabilitySettings.SectionName)
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.ServiceName), "Observability:ServiceName is required.")
            .Validate(settings => !string.IsNullOrWhiteSpace(settings.ServiceNamespace), "Observability:ServiceNamespace is required.")
            .ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Add DB context types for EFCore
    /// </summary>
    internal static IServiceCollection AddDbContext(this IServiceCollection services, IConfigurationRoot configuration)
    {
        string conn = configuration.GetConnectionString("SharedDb")
            ?? configuration.GetSection("Connections").GetValue<string>("SharedDb")
            ?? throw new InvalidOperationException("Connection string 'SharedDb' is not configured.");

        _ = services.AddDbContext<SharedDbContext>(o => o.UseNpgsql(conn));
        return services;
    }

    /// <summary>
    /// Add http clients.
    /// <see href="https://habr.com/ru/companies/dododev/articles/503376/" />
    /// </summary>
    internal static IServiceCollection AddHttpClients(this IServiceCollection services, IConfigurationRoot configuration)
    {
        HttpPolicies policies = configuration.GetSection(HttpPolicies.SectionName).Get<HttpPolicies>() ?? new HttpPolicies();
        _ = services
            .AddResilienceEnricher() // https://learn.microsoft.com/ru-ru/dotnet/core/resilience/?tabs=dotnet-cli#add-resilience-enrichment
            .AddHttpClient<ExampleHttpClient>(configureClient: static client => client.BaseAddress = new("http://localhost:9010"))
            .AddResilience(policies);

        return services;
    }

    internal static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfigurationRoot configuration)
    {
        Dictionary<string, string> connections = configuration
            .GetSection("ConnectionStrings")
            .Get<Dictionary<string, string>>()
            ?? [];

        IHealthChecksBuilder checks = services.AddHealthChecks();

        foreach (KeyValuePair<string, string> conn in connections)
        {
            if (string.IsNullOrWhiteSpace(conn.Value))
                continue;

            _ = checks
                .AddTypeActivatedCheck<PgSqlCheck>(
                    name: conn.Key,
                    args: [conn.Value]
            );
        }
        return services;
    }

    internal static IServiceCollection AddAndConfigureVersionedApi(this IServiceCollection services)
    {
        _ = services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
                options.JsonSerializerOptions.WriteIndented = true;
                options.JsonSerializerOptions.AllowTrailingCommas = true;
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        _ = services
            .AddOpenApi()
            .AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VV";
                options.SubstituteApiVersionInUrl = true;
            });

        _ = services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = ValidationHelper.GenerateValidationErrorResult);

        _ = services
            .AddExceptionHandler<GlobalExceptionHandler>()
            .AddProblemDetails(options =>
                options.CustomizeProblemDetails = context =>
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);

        return services;
    }

    internal static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfigurationRoot configuration)
    {
        CorsSettings settings = configuration
                .GetSection(CorsSettings.SectionName)
                .Get<CorsSettings>()
                ?? new CorsSettings();

        _ = services.AddCors(options => options.AddPolicy(_corsPolicyName, policy =>
            {
                if (settings.AllowedOrigins.Length > 0)
                    _ = policy
                            .WithOrigins(settings.AllowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
            }
        ));
        // Если frontend использует cookie authentication, в конец добавляем:
        // .AllowCredentials() 
        // Для JWT в заголовке Authorization обычно AllowCredentials() не требуется.
        // Достаточно
        // .AllowAnyHeader()
        // либо:
        // .WithHeaders("Content-Type", "Authorization")

        return services;
    }

    internal static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ObservabilitySettings settings = configuration
            .GetSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>()
            ?? new ObservabilitySettings();

        string serviceName = string.IsNullOrWhiteSpace(settings.ServiceName)
            ? environment.ApplicationName
            : settings.ServiceName;

        string serviceVersion = typeof(Program)
            .Assembly
            .GetName()
            .Version?
            .ToString()
            ?? "unknown";

        _ = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => _ = resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>(
                        "deployment.environment",
                        environment.EnvironmentName),
                    new KeyValuePair<string, object>(
                        "service.namespace",
                        settings.ServiceNamespace),
                ]))
            .AddTracing(settings, serviceName)
            .AddMetrics(settings);

        return services;
    }

    /// <summary>
    /// Add resilience policies.
    /// <see href="https://learn.microsoft.com/ru-ru/dotnet/core/resilience/http-resilience"/>
    /// </summary>
    private static IHttpStandardResiliencePipelineBuilder AddResilience(this IHttpClientBuilder builder, HttpPolicies httpPolicies)
    {
        return builder.AddStandardResilienceHandler(options =>
            {
                options.Retry.DisableForUnsafeHttpMethods(); // do not retry post/patch/put/delete
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.Retry.MaxRetryAttempts = httpPolicies.RetriesCount;
                options.Retry.DelayGenerator = args =>
                {
                    TimeSpan? customDelay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber));
                    return ValueTask.FromResult(customDelay);
                };

                options.TotalRequestTimeout = new HttpTimeoutStrategyOptions { Timeout = httpPolicies.OverallTimeout };
                options.AttemptTimeout = new HttpTimeoutStrategyOptions { Timeout = httpPolicies.TimeoutPerTry };
            });
    }

    /// <summary>
    /// Traces answer: where did this request spend time?
    /// ASP.NET Core instrumentation creates spans for incoming HTTP requests.
    /// HttpClient instrumentation creates spans for outgoing HTTP calls.
    /// </summary>
    private static OpenTelemetry.OpenTelemetryBuilder AddTracing(this OpenTelemetry.OpenTelemetryBuilder openTelemetryBuilder, ObservabilitySettings settings, string serviceName)
    {
        _ = openTelemetryBuilder.WithTracing(tracing =>
        {
            _ = tracing
                .AddAspNetCoreInstrumentation(options =>
                    options.RecordException = true)
                .AddHttpClientInstrumentation()
                .AddSource(serviceName);

            if (settings.EnableOtlpExporter)
            {
                // OTLP is the recommended vendor-neutral export path.
                // Endpoint/protocol can be configured by OTEL_EXPORTER_OTLP_* env vars.
                _ = tracing.AddOtlpExporter();
            }

            if (settings.EnableConsoleExporter)
            {
                // Useful only for local debugging.
                // Do not enable in production unless logs are intentionally noisy.
                _ = tracing.AddConsoleExporter();
            }
        });

        return openTelemetryBuilder;
    }

    /// <summary>
    /// Metrics answer: how often, how fast, how much?
    /// ASP.NET Core adds request metrics.
    /// HttpClient adds outgoing HTTP metrics.
    /// Runtime adds GC/threadpool/process runtime metrics.
    /// </summary>
    private static OpenTelemetry.OpenTelemetryBuilder AddMetrics(this OpenTelemetry.OpenTelemetryBuilder openTelemetryBuilder, ObservabilitySettings settings)
    {
        _ = openTelemetryBuilder
            .WithMetrics(metrics =>
            {
                _ = metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                if (settings.EnableOtlpExporter)
                {
                    // Sends metrics to OpenTelemetry Collector/APM backend.
                    _ = metrics.AddOtlpExporter();
                }

                if (settings.EnablePrometheusExporter)
                {
                    // Exposes metrics for Prometheus scraping.
                    // Requires app.MapPrometheusScrapingEndpoint() in endpoint mapping.
                    _ = metrics.AddPrometheusExporter();
                }

                if (settings.EnableConsoleExporter)
                {
                    // Useful only for local debugging.
                    _ = metrics.AddConsoleExporter();
                }
            });
        return openTelemetryBuilder;
    }
}
