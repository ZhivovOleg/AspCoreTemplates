using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using AspCore.Microservices.Template.Data;
using AspCore.Microservices.Template.Dto.AppSettings;
using AspCore.Microservices.Template.Services;

namespace AspCore.Microservices.Template.Middleware
{
	/// <summary>
	/// Extentions for ServiceCollection
	/// </summary>
	public static class ServiceCollectionExtentions
	{
		/// <summary>
		/// Add swagger 
		/// </summary>
		public static IServiceCollection AddSwagger(this IServiceCollection services, string swaggerApiName) =>
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo {Title = swaggerApiName, Version = "v1"});
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
	}
}