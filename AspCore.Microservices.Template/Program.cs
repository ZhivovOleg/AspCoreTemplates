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
        /// <summary>
        /// Start point
        /// </summary>
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Error()
                .WriteTo.File($"Logs/{nameof(Program)}.log")
                .CreateLogger();
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
                });
    }
}
