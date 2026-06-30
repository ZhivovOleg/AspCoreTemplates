using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.Controllers;

/// <summary>
/// Example controller
/// </summary>
/// <remarks>
/// DI ctor
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ExampleController(IExampleDatabaseService exampleService) : ControllerBase
{
    private readonly IExampleDatabaseService _exampleService = exampleService;

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
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetExample(ExampleRequestDto requestDto)
    {
        ExampleResponseDto result = await _exampleService.GetData(requestDto.Id);
        return Ok(result);
    }
}