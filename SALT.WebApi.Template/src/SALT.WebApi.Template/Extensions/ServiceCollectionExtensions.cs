using System;
using System.Collections.Generic;
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
        HttpPolicies policies = configuration.GetSection("HttpPolicies").Get<HttpPolicies>() ?? new HttpPolicies();
        _ = services
            .AddResilienceEnricher() // https://learn.microsoft.com/ru-ru/dotnet/core/resilience/?tabs=dotnet-cli#add-resilience-enrichment
            .AddHttpClient<ExampleHttpClient>(configureClient: static client => client.BaseAddress = new("http://localhost:9010"))
            .AddResilience(policies);

        return services;
    }

    internal static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfigurationRoot configuration)
    {
        Dictionary<string, string> connections = configuration.GetValue<Dictionary<string, string>>("ConnectionStrings");
        foreach (KeyValuePair<string, string> conn in connections)
        {
            _ = services.AddHealthChecks()
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

        if (settings.AllowedOrigins.Length == 0)
            return services;

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
            .ToString();

        _ = services
            .AddOpenTelemetry()

            // Resource describes this service instance.
            // Backends use these attributes to group telemetry by service,
            // environment, version, namespace, and other deployment metadata.
            .ConfigureResource(resource => _ = resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion)

                // Standard OpenTelemetry semantic convention.
                // Example: Development, Staging, Production.
                .AddAttributes([
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
                    // Records exceptions as span events.
                    // Useful when investigating failed requests in tracing backend.
                    options.RecordException = true)
                .AddHttpClientInstrumentation()

                // Add custom ActivitySource names here when business traces appear.
                // Business code should create ActivitySource with the same name passed to AddSource.
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