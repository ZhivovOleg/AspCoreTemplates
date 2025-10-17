using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using SALT.WebApi.Template.AppCore;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Dto.AppSettings;
using SALT.WebApi.Template.Net;
using SALT.WebApi.Template.Services;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SALT.WebApi.Template.Extensions;

/// <summary>
/// Extensions for ServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add swagger 
    /// </summary>
    public static IServiceCollection AddSwagger(this IServiceCollection services) =>
        services
            .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
            .AddSwaggerGen(c =>
            {
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

    /// <summary>
    /// Add logic 
    /// </summary>
    public static IServiceCollection AddLogicServices(this IServiceCollection services) =>
        services.AddTransient<IExampleDatabaseService, ExampleDatabaseService>();

    /// <summary>
    /// Add health checks
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, HealthChecksSettings healthChecksSettings)
    {
        TimeSpan timeout = TimeSpan.FromSeconds(healthChecksSettings.IntervalSeconds);
        IHealthChecksBuilder hcBuilder = services.AddHealthChecks();

        foreach (string connStr in healthChecksSettings.PgSql)
        {
            string name = connStr[..connStr.IndexOf(';', connStr.IndexOf("Database=", StringComparison.Ordinal))];
            _ = hcBuilder.AddNpgSql(connStr, name: name, timeout: timeout);
        }

        foreach (Host host in healthChecksSettings.Tcp)
            _ = hcBuilder.AddTcpHealthCheck(s => s.AddHost(host.Name, host.Port), host.Descsription, timeout: timeout);

        return services;
    }

    /// <summary>
    /// Add DB context types for EFCore
    /// </summary>
    public static IServiceCollection AddDbContext(this IServiceCollection services, Dictionary<string, string> connectionsSettings)
    {
        string connStr = connectionsSettings.First(x => x.Key == "SharedDb").Value;
        return services.AddDbContext<SharedDbContext>(o => o.UseNpgsql(connStr));
    }

    /// <summary>
    /// Add Api Versioning for Web API.
    /// </summary>
    public static IServiceCollection AddSupportApiVersioning(this IServiceCollection services)
    {
        _ = services.AddApiVersioning(options =>
        {
            // Add to http response headers:
            // api-supported-versions – list of supported API versions
            // api-deprecated-versions – list of Depricated API versions
            options.ReportApiVersions = true;
            // Use versioning by url segment 
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddVersionedApiExplorer(options =>
        {
            // Set versioning format from https://github.com/Microsoft/aspnet-api-versioning/wiki/Version-Format#custom-api-version-format-strings
            options.GroupNameFormat = "'v'VV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Add http clients.
    /// <see href="https://habr.com/ru/companies/dododev/articles/503376/" />
    /// </summary>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, HttpPolicies httpPolicies)
    {
        _ = services
            .AddResilienceEnricher() // https://learn.microsoft.com/ru-ru/dotnet/core/resilience/?tabs=dotnet-cli#add-resilience-enrichment
            .AddHttpClient<ExampleHttpClient>(configureClient: static client => client.BaseAddress = new("http://localhost:9010"))
            .AddResilience(httpPolicies);

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
}