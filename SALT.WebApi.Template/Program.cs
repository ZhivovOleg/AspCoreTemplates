using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Builder;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SALT.WebApi.Template.Services;
using SALT.WebApi.Template.Data;
using SALT.WebApi.Template.AppCore;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Salt.RequestHandler;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Main thread
/// </summary>
internal sealed class Program
{
    //TIP: For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError()'
    private static readonly Action<ILogger, Exception> _logError = LoggerMessage.Define(
        LogLevel.Critical,
        new EventId(1, nameof(Main)),
        "Ошибка выполнения"
        );

    private static IConfigurationRoot LoadConfiguration(WebApplicationBuilder builder) =>
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
            .Build();

    /// <summary>
    /// Entry point
    /// </summary>
    private static void Main(string[] args)
    {
        ILogger logger = LoggerFactory.Create(config => config.AddFile("Logs/{Date}.json", isJson: true)).CreateLogger("Program");

        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Environment.ContentRootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            IConfigurationRoot configuration = LoadConfiguration(builder);

            ConfigureWebServer(builder.WebHost, configuration);

            AddAndConfigureVersionedApi(builder.Services);

            AddSwagger(builder.Services);

            AddHealthChecks(builder.Services, configuration);

            AddBusinessLogic(builder.Services, configuration);

            PrepareWebApp(builder, configuration).Run();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancellation is requested");
        }
        catch (Exception exc)
        {
            _logError(logger, exc);
            throw;
        }
    }

    private static void ConfigureWebServer(ConfigureWebHostBuilder builder, IConfigurationRoot configuration)
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

    private static void AddAndConfigureVersionedApi(IServiceCollection services)
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

        _ = services.AddApiVersioning(options =>
        {
            options.ReportApiVersions = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.RegisterMiddleware = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new HeaderApiVersionReader("api-version");
        });

        _ = services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = ValidationHelper.GenerateValidationErrorResult);
    }

    private static void AddSwagger(IServiceCollection services)
    {
        _ = services
            .AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV")
            .ConfigureOptions<ConfigureSwaggerOptions>()
            .AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());
    }

    private static void AddHealthChecks(IServiceCollection services, IConfigurationRoot configuration)
    {
        _ = services.AddHealthChecks()
            .AddTypeActivatedCheck<PgSqlCheck>(
                name: "Storage Connection",
                args: new string[] { configuration.GetSection("Connections").GetValue<string>("OperationalStorage") }
            );
    }

    private static void AddBusinessLogic(IServiceCollection services, IConfigurationRoot configuration)
    {
        _ = services
            .AddDbContext<SharedDbContext>(o => o.UseNpgsql(configuration.GetSection("Connections").GetValue<string>("storage")))
            .AddTransient<IExampleService, ExampleService>();
    }

    private static WebApplication PrepareWebApp(WebApplicationBuilder builder, IConfigurationRoot configuration)
    {
        WebApplication app = builder.Build();

        if (configuration.GetSection("Kestrel").GetValue<bool>("UseReverseProxy"))
            _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

        _ = app.Environment.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseHsts().UseStatusCodePages();

        _ = app
            .AddRequestHandler(RequestHandlerPolicy.HANDLE_ONLY_CRASHED)
            .UseCors(options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
            .UseSwagger()
            .UseSwaggerUI()
            .UseHttpsRedirection()
            .UseAuthorization();

        _ = app.MapControllers();

        _ = app.MapHealthChecks("/healthz", new HealthCheckOptions { ResponseWriter = HealthCheckHelpers.WriteJsonResponse });

        return app;
    }
}