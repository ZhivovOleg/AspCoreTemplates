using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Валидатор входящих DTO
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Генерирует модель ошибки согласно 
    /// <see href="https://github.com/ZhivovOleg/salt-api-specification/blob/master/document/errors.md">Salt-Api</see>
    /// </summary>
    public static IActionResult GenerateValidationErrorResult(ActionContext context)
    {
        ValidationProblemDetails problem = new(context.ModelState)
        {
            Status = StatusCodes.Status422UnprocessableEntity,
            Title = "Ошибка валидации",
            Instance = context.HttpContext.Request.Path,
        };

        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new UnprocessableEntityObjectResult(problem);
    }
}