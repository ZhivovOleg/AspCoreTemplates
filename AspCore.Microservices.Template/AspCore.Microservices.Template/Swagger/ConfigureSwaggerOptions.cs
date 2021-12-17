using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspCore.Microservices.Template.Swagger;

/// <inheritdoc />
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary> .ctor </summary>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => _provider = provider;

    /// <inheritdoc />
    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in _provider.ApiVersionDescriptions)
        {
            OpenApiInfo info = new()
            {
                Title = "AspCore.Microservices.Template",
                Version = description.ApiVersion.ToString(),
                Description = "Base microservice template."
            };

            if (description.IsDeprecated)
            {
                info.Description += " Warning! API version is depricated.";
            }

            options.SwaggerDoc(description.GroupName, info);
        }
    }
}