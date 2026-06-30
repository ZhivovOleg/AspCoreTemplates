using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Converts unhandled exceptions to RFC 7807 problem details responses.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    private readonly IHostEnvironment _environment = environment;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        LogUnhandledException(
            _logger,
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier,
            exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = _environment.IsDevelopment() ? exception.Message : null,
                Instance = httpContext.Request.Path,
            },
        }).ConfigureAwait(false);
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}")]
    private static partial void LogUnhandledException(
        ILogger logger,
        string method,
        string path,
        string traceId,
        Exception exception);
}
