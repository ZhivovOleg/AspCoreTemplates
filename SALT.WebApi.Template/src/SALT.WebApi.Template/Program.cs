using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SALT.WebApi.Template.Extensions;


namespace SALT.WebApi.Template;

/// <summary>
/// Main thread
/// </summary>
internal sealed class Program
{
    private static IConfigurationRoot LoadConfiguration(WebApplicationBuilder builder) =>
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .Build();

    /// <summary>
    /// For xunit
    /// </summary>
    private Program()
    { }

    /// <summary>
    /// Entry point
    /// </summary>
    private static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Environment.ContentRootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        IConfigurationRoot configuration = LoadConfiguration(builder);

        _ = builder.Services
            .AddCorsConfiguration(configuration)
            .AddAndConfigureVersionedApi()
            .AddHealthChecks(configuration)
            .AddDbContext(configuration)
            .AddHttpClients(configuration)
            .AddBusinessLogic(configuration);

        builder.WebHost.ConfigureWebServer(configuration);

        builder
            .Build()
            .UseMiddleware(configuration)
            .MapEndpoints()
            .Run();
    }
}