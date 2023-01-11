using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Services;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using SALT.WebApi.Template.AppCore.Dto;
using System.Globalization;
using Newtonsoft.Json;

namespace SALT.WebApi.Template.Controllers;

/// <summary>
/// Example controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ExampleController : ControllerBase
{
    private const string _errorTemplate = "Ошибка выполнения {0}";

    //TIP: For improved performance, use the LoggerMessage delegates instead of calling 'LoggerExtensions.LogError()'
    private static readonly Action<ILogger, Exception> _logError = LoggerMessage.Define(
        LogLevel.Critical,
        new EventId(1, nameof(GetExample)),
        "Ошибка выполнения"
        );

    private readonly IExampleService _exampleService;

    private readonly ILogger<ExampleController> _logger;

    /// <summary>
    /// DI ctor
    /// </summary>
    public ExampleController(IExampleService exampleService, ILogger<ExampleController> logger)
    {
        _exampleService = exampleService;
        _logger = logger;
    }

    /// <summary>
    /// Generate example value
    /// </summary>
    /// <param name="requestDto">Объект запроса</param>
    /// <returns>Сгенерированное случайное число</returns>
    /// <remarks>
    /// Пример запроса:<br/>
    /// <br/>
    /// POST http://localhost:9000/api/v1/Example/GetExample<br/>
    /// Content-Type: application/json; charset=utf-8<br/>
    /// api-version: 1.0<br/>
    /// <br/>
    /// <pre>
    /// {
    ///     "id": 1,
    /// }
    /// </pre>
    /// </remarks>
    /// <response code="200">случайное число</response>
    /// <response code="422">Ошибка валидации</response>
    /// <response code="500">Ошибка обработки</response>

    [HttpPost]
    [Produces("application/json")]
    [Route(nameof(GetExample))]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ExampleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ErrorDto), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetExample(ExampleRequestDto requestDto)
    {
        try
        {
            ExampleResponseDto result = await _exampleService.GetData(requestDto.Id);
            return Ok(result);
        }
        catch (Exception exc)
        {
            ErrorDto error = new()
            {
                Title = string.Format(CultureInfo.InvariantCulture, _errorTemplate, exc.Message),
                Source = new()
                {
                    Detail = exc.StackTrace,
                    Action = nameof(GetExample),
                    Parameters = JsonConvert.SerializeObject(requestDto)
                }
            };
            _logError(_logger, exc);
            return StatusCode(500, error);
        }
    }
}