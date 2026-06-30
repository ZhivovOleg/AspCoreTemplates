using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using SALT.WebApi.Template.AppCore;
using Scalar.AspNetCore;

namespace SALT.WebApi.Template.Extensions;

/// <summary>
/// ApplicationBuilder extentions
/// </summary>
internal static class AppBuilderExtensions
{
    internal static void ConfigureWebServer(this ConfigureWebHostBuilder builder, IConfigurationRoot configuration)
    {
        TimeSpan keepAliveTimeout = TimeSpan.FromSeconds(configuration.GetSection("Kestrel").GetValue<int>("KeepAliveTimeout"));
        TimeSpan requestHeadersTimeout = TimeSpan.FromSeconds(configuration.GetSection("Kestrel").GetValue<int>("RequestHeadersTimeout"));
        int port = configuration.GetSection("Kestrel").GetValue<int>("Port");

        _ = builder.UseKestrel((context, kestrelServerOptions) =>
        {
            kestrelServerOptions.AllowSynchronousIO = false;
            kestrelServerOptions.Limits.MaxRequestBodySize = null;
            kestrelServerOptions.Limits.KeepAliveTimeout = keepAliveTimeout;
            kestrelServerOptions.Limits.RequestHeadersTimeout = requestHeadersTimeout;
            kestrelServerOptions.ListenAnyIP(port);
        });
    }

    internal static WebApplication MapEndpoints(this WebApplication webApp)
    {
        _ = webApp
            .UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
            .UseHttpsRedirection()
            .UseAuthorization();

        _ = webApp.MapControllers();

        _ = webApp.MapHealthChecks("/healthz", new HealthCheckOptions { ResponseWriter = HealthCheckHelpers.WriteJsonResponse });

        return webApp;
    }

    internal static WebApplication UseMiddleware(this WebApplication webApp, IConfigurationRoot configuration)
    {
        if (configuration.GetSection("Kestrel").GetValue<bool>("UseReverseProxy"))
            _ = webApp.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            });

        _ = webApp
            .UseExceptionHandler()
            .UseCors(ServiceCollectionExtensions._corsPolicyName);

        if (webApp.Environment.IsDevelopment())
        {
            _ = webApp.MapOpenApi();
            _ = webApp.MapScalarApiReference();
        }
        else
            _ = webApp.UseHsts().UseStatusCodePages();

        _ = webApp.MapPrometheusScrapingEndpoint();

        return webApp;
    }

    internal static WebApplicationBuilder UseOltpLogs(this WebApplicationBuilder builder)
    {
        _ = builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
            options.ParseStateValues = true;
            _ = options.AddOtlpExporter();
        });

        return builder;
    }
}