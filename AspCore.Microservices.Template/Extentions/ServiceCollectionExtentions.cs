using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Dto.AppSettings;
using AspCore.Microservices.Template.Services;
using AspCore.Microservices.Template.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AspCore.Microservices.Template.Extentions
{
	/// <summary>
	/// Extentions for ServiceCollection
	/// </summary>
	public static class ServiceCollectionExtentions
	{
		/// <summary>
		/// Add swagger 
		/// </summary>
		public static IServiceCollection AddSwagger(this IServiceCollection services)
		{
			services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
			services.AddSwaggerGen(c =>
			{
				string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				c.IncludeXmlComments(xmlPath);
			});

			return services;
		}

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
			    string name = connStr.Substring(0, connStr.IndexOf(';', connStr.IndexOf("Database=", StringComparison.Ordinal)));
			    hcBuilder.AddNpgSql(connStr, name: name, timeout: timeout);
		    }

		    foreach (Host host in healthChecksSettings.Tcp)
			    hcBuilder.AddTcpHealthCheck(s => s.AddHost(host.Name, host.Port), host.Descsription, timeout: timeout);
		    
		    return services;
		}

		/// <summary>
		/// Add DB context types for EFCore
		/// </summary>
		public static IServiceCollection AddDbContext(this IServiceCollection services, Dictionary<string,string> connectionsSettings)
		{
			string connStr = connectionsSettings.First(x => x.Key == "SharedDb").Value;
			return services.AddDbContext<SharedDbContext>(o => o.UseNpgsql(connStr));
		}
		
		/// <summary>
		/// Add Api Versioning for Web API.
		/// </summary>
		public static IServiceCollection AddSupportApiVersioning(this IServiceCollection services)
		{
			services.AddApiVersioning(options =>
			{
				// Add to http response headers:
				// api-supported-versions – list of supported API versions
				// api-deprecated-versions – list of Depricated API versions
				options.ReportApiVersions = true;

				// Use versioning by url segment 
				options.ApiVersionReader = new UrlSegmentApiVersionReader();
			});

			services.AddVersionedApiExplorer(options =>
			{
				// Set versioning format from https://github.com/Microsoft/aspnet-api-versioning/wiki/Version-Format#custom-api-version-format-strings
				options.GroupNameFormat = "'v'VV";

				options.SubstituteApiVersionInUrl = true;
			});

			return services;
		}
	}
}