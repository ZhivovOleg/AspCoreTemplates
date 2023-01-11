using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Salt.RequestHandler;

namespace SALT.WebApi.Template.Extensions;

/// <summary>
/// ApplicationBuilder extentions
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Add basic services and middleware. DO NOT CHANGE!
    /// </summary>
    public static IApplicationBuilder AddBaseFunctions(
        this IApplicationBuilder app,
        IWebHostEnvironment env,
        IConfiguration configuration)
    {
        if (configuration.GetSection("Kestrel").GetValue<bool>("UseReverseProxy"))
            _ = app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

        _ = env.IsDevelopment() ? app.UseDeveloperExceptionPage() : app.UseHsts().UseStatusCodePages();

        return app.AddRequestHandler(RequestHandlerPolicy.HANDLE_ONLY_CRASHED)
            .UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod())
            .UseRouting()
            .UseEndpoints(endpoints =>
            {
                _ = endpoints.MapControllers();
                _ = endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
                {
                    AllowCachingResponses = false,
                    ResponseWriter = (c, r) =>
                    {
                        c.Response.ContentType = "application/json; charset=utf-8";
                        if (r.Status == HealthStatus.Healthy)
                        {
                            c.Response.ContentType = "text";
                            return c.Response.WriteAsync(HealthStatus.Healthy.ToString());
                        }
                        c.Response.ContentType = "application/json; charset=utf-8";
                        string json = JsonConvert.SerializeObject(r, Formatting.Indented);
                        return c.Response.WriteAsync(json);
                    }
                });
            });
    }

    /// <summary>
    /// Use swagger with UI
    /// </summary>
    public static IApplicationBuilder UseSwaggerWithUi(this IApplicationBuilder app, IApiVersionDescriptionProvider provider) =>
        app.UseSwagger()
            .UseSwaggerUI(options =>
            {
                // Set swagger.json for each API version.
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"../swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });
}