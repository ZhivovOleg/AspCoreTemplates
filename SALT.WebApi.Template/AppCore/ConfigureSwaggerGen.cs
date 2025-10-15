using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Конфигурирование swagger для поддержи нескольких версий API
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));
    }

    /// <inheritdoc />
    public void Configure(string name, SwaggerGenOptions options) => Configure(options);

    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description)
    {
        OpenApiInfo info = new()
        {
            Title = "SALT.WebApi.Template",
            Version = description.ApiVersion.ToString(),
            Description = "Base microservice template."
        };

        if (description.IsDeprecated)
        {
            info.Description += " Warning! API version is depricated.";
        }

        return info;
    }
}