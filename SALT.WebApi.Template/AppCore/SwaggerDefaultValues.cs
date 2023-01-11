using System.Linq;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Небольшое исправление настройки swagger <br />
/// <see href="ttps://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412">REF</see>
/// <see href="https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413">REF</see>
/// </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ApiDescription apiDescription = context.ApiDescription;
        operation.Deprecated |= apiDescription.IsDeprecated();

        if (operation.Parameters == null)
            return;

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        foreach (OpenApiParameter parameter in operation.Parameters)
        {
            ApiParameterDescription description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
            parameter.Description ??= description.ModelMetadata?.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
                parameter.Schema.Default = new OpenApiString(description.DefaultValue.ToString());

            parameter.Required |= description.IsRequired;
        }
    }
}