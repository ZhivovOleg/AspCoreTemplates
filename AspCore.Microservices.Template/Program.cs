using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace AspCore.Microservices.Template
{
    /// <summary>
    /// Main thread
    /// </summary>
    public class Program
    {
	    private static void ConfigureLogging()
	    {
		    try
		    {
			    IConfigurationRoot configuration = new ConfigurationBuilder()
				    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				    .AddJsonFile(
					    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
					    optional: true)
				    .Build();

			    Log.Logger = new LoggerConfiguration()
				    .ReadFrom.Configuration(configuration)
				    .CreateLogger();
		    }
		    catch (Exception exc)
		    {
			    Log.Logger = new LoggerConfiguration()
				    .MinimumLevel.Error()
				    .WriteTo.File("Logs/{Date}.log")
				    .CreateLogger();
		        
			    Log.Logger.Error(exc, "Error while configure Serilog by appsettings. File logging will be used");
		    }
	    }
	    
        /// <summary>
        /// Start point
        /// </summary>
        public static void Main(string[] args)
        {
	        ConfigureLogging();
	        
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancellation is requested");
            }
            catch (Exception exc)
            {
                Log.Error(exc, "Stopped program because of exception : " + exc.Message);
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Build web app
        /// </summary>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseContentRoot(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location))
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
	                IHostEnvironment env = hostingContext.HostingEnvironment;

	                config
		                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
		                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel((hostingContext, options) =>
                    {
                        options.AllowSynchronousIO = false;
                        int port = hostingContext.Configuration.GetSection("Kestrel").GetValue<int>("Port");
                        options.ListenAnyIP(port);
                        options.Limits.MaxRequestBodySize = null;
                        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(60);
                        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(60);
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .UseSerilog();
    }
}
