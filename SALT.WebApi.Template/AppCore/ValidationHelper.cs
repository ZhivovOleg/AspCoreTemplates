using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SALT.WebApi.Template.AppCore.Dto;

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
        Dictionary<string, object> errors = context.ModelState
            .Where(entry => entry.Value.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
            .ToDictionary(x => x.Key, x => x.Value.Errors.Select(y => y.ErrorMessage) as object);

        ErrorDto result = new()
        {
            Title = "Ошибка валидации",
            Source = new()
            {
                Action = context.HttpContext.Request.Path,
                Parameters = JsonConvert.SerializeObject(errors)
            }
        };
        JsonResult actionResult = new(result)
        {
            StatusCode = 422
        };
        return actionResult;
    }
}