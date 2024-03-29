﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.Dto.AppSettings;
using SALT.WebApi.Template.Services;
using SALT.WebApi.Template.Swagger;
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
        services.AddTransient<IExampleService, ExampleService>();

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
}