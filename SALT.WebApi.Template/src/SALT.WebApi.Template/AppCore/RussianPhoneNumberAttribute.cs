using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace SALT.WebApi.Template.AppCore;

/// <summary>
/// Атрибут для проверки номера телефона. <br/>
/// <see href="https://github.com/microsoft/referencesource/blob/master/System.ComponentModel.DataAnnotations/DataAnnotations/PhoneAttribute.cs">
/// Основано на System.ComponentModel.DataAnnottions.PhoneAttribute
/// </see>
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class RussianPhoneNumberAttribute : DataTypeAttribute
{
    private const string _errorMessage = "Некорректный номер телефона";

    /// <summary>
    /// Проверяет корректность номера телефона. <br/>
    /// Игнорируются символы ['+', '-', '(', ')', '.'] <br/>
    /// При наличии иных символов, кроме цифр, выпадает ошибка валидации <br/>
    /// Игнорируются ведущие цифры '7' и '8'. <br/>
    /// если итоговых цифр менее 10 выпадает ошибка валидации
    /// </summary>
    public RussianPhoneNumberAttribute() : base(DataType.PhoneNumber) =>
        ErrorMessage = _errorMessage;

    /// <inheritdoc />
    public override bool IsValid(object value)
    {
        string valueAsString = value as string;

        if (string.IsNullOrWhiteSpace(valueAsString))
            return false;

        valueAsString = valueAsString
            .Trim()
            .TrimStart('+')
            .TrimStart('7')
            .TrimStart('8')
            .Replace("(", string.Empty, StringComparison.Ordinal)
            .Replace(")", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace(".", string.Empty, StringComparison.Ordinal);

        return valueAsString.Length == 10 && valueAsString.All(char.IsDigit);
    }
}
