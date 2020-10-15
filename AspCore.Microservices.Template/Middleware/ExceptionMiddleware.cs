using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspCore.Microservices.Template.Dto.AppSettings;

namespace AspCore.Microservices.Template.Middleware
{
	/// <summary>
	/// Middleware for advanced exceptions handling
	/// </summary>
	public class ExceptionMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ExceptionMiddleware> _logger;
		private readonly AdvancedExceptionsHandlingSettings _advancedExceptionsHandlingSettings;

		private async Task SaveRequestToFile(HttpRequest request)
		{
			try
			{
				string currentTime = DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
				string logsPath = "Logs/ErrorsRequests";
				if (!Directory.Exists(logsPath))
					Directory.CreateDirectory(logsPath);
				StringBuilder recordSb = new StringBuilder();
				recordSb.Append(request.Path);
				recordSb.Append(request.QueryString);
				recordSb.Append(Environment.NewLine);

				if(request.ContentLength != null && request.ContentLength > 0)
					using (StreamReader reader = new StreamReader(
						request.Body,
						encoding: Encoding.UTF8,
						detectEncodingFromByteOrderMarks: false,
						bufferSize: Convert.ToInt32(request.ContentLength),
						leaveOpen: true))
					{
						request.Body.Position = 0;
						string body = await reader.ReadToEndAsync();
						recordSb.Append(Environment.NewLine);
						recordSb.Append(body);
						request.Body.Position = 0; // Reset the request body stream position so the next middleware can read it
					}

				File.WriteAllText($"Logs/ErrorsRequests/request_{currentTime}.json", recordSb.ToString(), Encoding.UTF8);
			}
			catch (Exception exc)
			{
				_logger.LogError($"Error on writing request to file: {exc.Message}", exc);
				throw;
			}
		}
		
		/// <summary>
		/// DI ctor
		/// </summary>
		public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IOptions<AppSettings> appSettings)
		{
			_next = next;
			_logger = logger;
			_advancedExceptionsHandlingSettings = appSettings.Value?.Logging?.AdvancedExceptionsHandling;
		}

		/// <summary>
		/// Basic middleware method
		/// </summary>
		public async Task Invoke(HttpContext context)
		{
			try
			{
				if(_advancedExceptionsHandlingSettings?.SaveRequestBodyOnErrors == true)
					context.Request.EnableBuffering(); //enabling re-reading requests
				await _next.Invoke(context);
			}
			catch(Exception ex)
			{
				if(_advancedExceptionsHandlingSettings?.SaveRequestBodyOnErrors == true)
					await SaveRequestToFile(context.Request);
				_logger.LogError(ex, ex.Message);
				context.Response.StatusCode = 500;
				context.Response.ContentType = "application/text";
				await context.Response.WriteAsync(ex.Message);
			}       
		}
	}
}