namespace SALT.WebApi.Template.AppCore.Dto;

/// <summary>
/// Ошибка
/// </summary>
public class ErrorDto
{
    /// <summary>
    /// Описание ошибки
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// Подробые сведения об ошибке
    /// </summary>
    public Source Source { get; set; }
}
