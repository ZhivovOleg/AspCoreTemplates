using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.Controllers;

/// <summary>
/// Example controller.
/// </summary>
/// <remarks>
/// Demonstrates controller-based API documentation.
/// </remarks>
[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
public class ExampleController(IExampleDatabaseService exampleService) : ControllerBase
{
    private readonly IExampleDatabaseService _exampleService = exampleService;

    /// <summary>
    /// Returns example data.
    /// </summary>
    /// <param name="id">Example entity identifier.</param>
    /// <returns>Example data from the shared database.</returns>
    /// <remarks>
    /// Example request:<br/>
    /// <br/>
    /// GET http://localhost:9000/api/Example/1<br/>
    /// api-version: 1.0<br/>
    /// </remarks>
    /// <response code="200">Example data was found.</response>
    /// <response code="422">Request validation failed.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpGet("{id:int}")]
    [Produces("application/json")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ExampleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetExample(int id)
    {
        if (id <= 0)
            return ValidationProblem(
                new ValidationProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = "Request validation failed.",
                    Errors =
                    {
                        [nameof(id)] = ["Id must be greater than zero."],
                    },
                });

        ExampleResponseDto result = await _exampleService.GetData(id);
        return Ok(result);
    }

    /// <summary>
    /// Processes example data.
    /// </summary>
    /// <param name="requestDto">Example request body.</param>
    /// <returns>Processing result. No data is persisted.</returns>
    /// <remarks>
    /// Example request:<br/>
    /// <br/>
    /// POST http://localhost:9000/api/Example<br/>
    /// Content-Type: application/json; charset=utf-8<br/>
    /// api-version: 1.0<br/>
    /// <br/>
    /// <pre>
    /// {
    ///     "id": 1
    /// }
    /// </pre>
    /// </remarks>
    /// <response code="200">Example request was processed.</response>
    /// <response code="422">Request validation failed.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost]
    [Produces("application/json")]
    [MapToApiVersion("1.0")]
    [ProducesResponseType(typeof(ExampleResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public IActionResult ProcessExample(ExampleRequestDto requestDto)
    {
        if (requestDto.Id <= 0)
            return ValidationProblem(
                new ValidationProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = "Request validation failed.",
                    Errors =
                    {
                        [nameof(requestDto.Id)] = ["Id must be greater than zero."],
                    },
                });

        ExampleResponseDto result = new()
        {
            Id = requestDto.Id,
            Result = "Example request was processed. No data was persisted.",
        };

        return Ok(result);
    }
}